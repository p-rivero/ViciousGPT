using NAudio.Wave;

namespace ViciousGPT.AudioProcessing;

// https://github.com/mihaits/NAudio-Synth

public abstract class WaveEffect32 : WaveProvider32
{
    protected ISampleProvider Input { get; }

    public float Wet { get; set; }

    protected WaveEffect32(int sampleRate, int channels) : base(sampleRate, channels) { }

    protected WaveEffect32(ISampleProvider input) : base(input.WaveFormat.SampleRate, input.WaveFormat.Channels)
    {
        Input = input;
        Wet = 1;
    }

    public override int Read(float[] buffer, int offset, int sampleCount)
    {
        var samplesRead = Input?.Read(buffer, offset, sampleCount) ?? sampleCount;

        if (Math.Abs(Wet) < float.Epsilon) return samplesRead;

        if (Math.Abs(Wet - 1) < float.Epsilon)
        {
            for (var i = 0; i < sampleCount; ++i)
                buffer[offset + i] = Apply(buffer[offset + i]);

            return samplesRead;
        }

        for (var i = 0; i < sampleCount; ++i)
            buffer[offset + i] = Apply(buffer[offset + i]) * Wet + buffer[offset + i] * (1 - Wet);

        return samplesRead;
    }

    public abstract float Apply(float sample);

}

public class LowPassFeedBackCombFilter : WaveEffect32
{
    private int _stereoSpread;
    private readonly float[] _xl, _yl;
    private float[] _xr, _yr;
    private float _xlnN1, _xrnN1;
    private int _posl, _posr;
    private bool _left;

    public LowPassFeedBackCombFilter(WaveProvider32 input, int delay, float f, float d, int stereoSpread) : base(input)
    {
        _left = true;

        _stereoSpread = stereoSpread;
        Feedback = f;
        Damping = d;

        _xl = new float[delay];
        _xr = new float[delay + stereoSpread];
        _yl = new float[delay];
        _yr = new float[delay + stereoSpread];
    }

    public float Feedback { get; set; }

    public float Damping { get; set; }

    public int StereoSpread
    {
        get => _stereoSpread;
        set
        {
            _stereoSpread = value;

            Array.Resize(ref _xr, _xl.Length + _stereoSpread);
            Array.Resize(ref _yr, _yl.Length + _stereoSpread);
        }
    }

    // y[n] = (1 - d) * f * y[n-N] + d * y[n-1] - d * x[n-(N+1)] + x[n-N]
    public override float Apply(float sample)
    {
        float yn;

        if (_left)
        {
            yn = (1 - Damping) * Feedback * _yl[_posl] + Damping * _yl[_posl == 0 ? _yl.Length - 1 : _posl - 1] - Damping * _xlnN1 + _xl[_posl];

            _xlnN1 = _xl[_posl];
            _xl[_posl] = sample;
            _yl[_posl] = yn;

            _posl = (_posl + 1) % _xl.Length;
        }
        else
        {
            yn = (1 - Damping) * Feedback * _yr[_posr] + Damping * _yr[_posr == 0 ? _yr.Length - 1 : _posr - 1] - Damping * _xrnN1 + _xr[_posr];

            _xrnN1 = _xr[_posr];
            _xr[_posr] = sample;
            _yr[_posr] = yn;

            _posr = (_posr + 1) % _xr.Length;
        }

        _left = !_left;

        return yn;
    }
}

public class AllPassFilterAprox : WaveEffect32
{
    private readonly float[] _xl, _xr, _yl, _yr;
    private int _posl, _posr;
    private bool _left;

    public AllPassFilterAprox(WaveProvider32 input, int delay, float gain, int stereoSpread) : base(input)
    {
        _left = true;

        Gain = gain;

        _xl = new float[delay];
        _xr = new float[delay + stereoSpread];
        _yl = new float[delay];
        _yr = new float[delay + stereoSpread];

        for (var i = 0; i < delay; ++i)
            _xl[i] = _xr[i] = _yl[i] = _yr[i] = 0;
        for (var i = delay; i < delay + stereoSpread; ++i)
            _xr[i] = _yr[i] = 0;

        _posl = _posr = 0;
    }

    public float Gain { get; set; }

    // y[n] = g * y[n-delay] + (1 + g) * x[n-delay] - x[n]
    public override float Apply(float sample)
    {
        float yn;

        if (_left)
        {
            yn = Gain * _yl[_posl] + (1 + Gain) * _xl[_posl] - sample;

            _xl[_posl] = sample;
            _yl[_posl] = yn;

            _posl = (_posl + 1) % _xl.Length;
        }
        else
        {
            yn = Gain * _yr[_posr] + (1 + Gain) * _xr[_posr] - sample;

            _xr[_posr] = sample;
            _yr[_posr] = yn;

            _posr = (_posr + 1) % _xr.Length;
        }

        _left = !_left;

        return yn;
    }
}

public enum MixerMode { Additive, Averaging }

public class WaveMixer32 : WaveProvider32
{
    private readonly List<WaveProvider32> _inputs, _toAdd, _toRemove;

    public MixerMode Mode { get; set; }

    public WaveMixer32(int sampleRate, int channels) : base(sampleRate, channels)
    {
        _inputs = new List<WaveProvider32>();
        _toAdd = new List<WaveProvider32>();
        _toRemove = new List<WaveProvider32>();

        Mode = MixerMode.Additive;
    }

    public WaveMixer32(WaveProvider32 firstInput) : this(firstInput.WaveFormat.SampleRate, firstInput.WaveFormat.Channels)
    {
        _inputs.Add(firstInput);
    }

    public void AddInput(WaveProvider32 waveProvider)
    {
        if (!waveProvider.WaveFormat.Equals(WaveFormat))
            throw new ArgumentException("All incoming channels must have the same format", "waveProvider.WaveFormat");

        _toAdd.Add(waveProvider);
    }

    public void AddInputs(IEnumerable<WaveProvider32> inputs)
    {
        inputs.ToList().ForEach(AddInput);
    }

    public void RemoveInput(WaveProvider32 waveProvider)
    {
        _toRemove.Add(waveProvider);
    }

    public int InputCount => _inputs.Count;

    public override int Read(float[] buffer, int offset, int count)
    {
        if (_toAdd.Count != 0)
        {
            _toAdd.ForEach(input => _inputs.Add(input));
            _toAdd.Clear();
        }
        if (_toRemove.Count != 0)
        {
            _toRemove.ForEach(input => _inputs.Remove(input));
            _toRemove.Clear();
        }

        for (var i = 0; i < count; ++i)
            buffer[offset + i] = 0;

        var readBuffer = new float[count];

        foreach (var input in _inputs)
        {
            input.Read(readBuffer, 0, count);

            for (var i = 0; i < count; ++i)
                buffer[offset + i] += readBuffer[i];
        }

        if (Mode == MixerMode.Averaging && _inputs.Count != 0)
            for (var i = 0; i < count; ++i)
                buffer[offset + i] /= _inputs.Count;

        return count;
    }
}

public class DummyWaveProvider32 : WaveProvider32
{
    private float[] _buffer;

    public DummyWaveProvider32(int sampleRate, int channels) : base(sampleRate, channels) { }

    public void SetBuffer(float[] buffer, int offset, int sampleCount)
    {
        _buffer = new float[sampleCount];
        for (var i = 0; i < sampleCount; ++i)
            _buffer[i] = buffer[offset + i];
    }

    public float[] GetBuffer()
    {
        return _buffer;
    }

    public override int Read(float[] buffer, int offset, int sampleCount)
    {
        if (buffer.Length != sampleCount)
            throw new Exception();

        for (var i = 0; i < sampleCount; ++i)
            buffer[offset + i] = _buffer[i];

        return sampleCount;
    }
}


public class Freeverb : WaveEffect32
{
    private readonly LowPassFeedBackCombFilter[] _lbcf;
    private readonly AllPassFilterAprox[] _ap;
    private readonly DummyWaveProvider32 _inputCopy;

    public float RoomSize
    {
        get => _lbcf[0].Feedback;
        set
        {
            foreach (var comb in _lbcf)
                comb.Feedback = value;
        }

    }

    public float Damping
    {
        get => _lbcf[0].Damping;
        set
        {
            foreach (var comb in _lbcf)
                comb.Damping = value;
        }
    }

    // input x4 -> _lbcf[0-4] -> _mixer[0] \
    //                                      > _mixer[2] -> _ap[0] -> _ap[1] -> _ap[2] -> _ap[3] -> output
    // input x4 -> _lbcf[5-7] -> _mixer[1] /
    public Freeverb(ISampleProvider input, float roomSize, float damping) : base(input)
    {
        _inputCopy = new DummyWaveProvider32(input.WaveFormat.SampleRate, input.WaveFormat.Channels);

        _lbcf = new[]
        {
                new LowPassFeedBackCombFilter(_inputCopy, 1557 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1617 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1491 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1422 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1277 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1356 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1188 * WaveFormat.SampleRate / 44100, roomSize, damping, 23),
                new LowPassFeedBackCombFilter(_inputCopy, 1116 * WaveFormat.SampleRate / 44100, roomSize, damping, 23)
            };

        var mixers = new[]
        {
                new WaveMixer32(input.WaveFormat.SampleRate, input.WaveFormat.Channels) {Mode = MixerMode.Averaging},
                new WaveMixer32(input.WaveFormat.SampleRate, input.WaveFormat.Channels) {Mode = MixerMode.Averaging},
                new WaveMixer32(input.WaveFormat.SampleRate, input.WaveFormat.Channels) {Mode = MixerMode.Averaging}
            };

        mixers[0].AddInputs(new[] { _lbcf[0], _lbcf[1], _lbcf[2], _lbcf[3] });
        mixers[1].AddInputs(new[] { _lbcf[4], _lbcf[5], _lbcf[6], _lbcf[7] });
        mixers[2].AddInputs(new[] { mixers[0], mixers[1] });

        _ap = new AllPassFilterAprox[4];

        _ap[0] = new AllPassFilterAprox(mixers[2], 225 * WaveFormat.SampleRate / 44100, .5f, 23);
        _ap[1] = new AllPassFilterAprox(_ap[0], 556 * WaveFormat.SampleRate / 44100, .5f, 23);
        _ap[2] = new AllPassFilterAprox(_ap[1], 441 * WaveFormat.SampleRate / 44100, .5f, 23);
        _ap[3] = new AllPassFilterAprox(_ap[2], 341 * WaveFormat.SampleRate / 44100, .5f, 23);
    }

    public override int Read(float[] buffer, int offset, int sampleCount)
    {
        int inputSamplesRead = Input.Read(buffer, offset, sampleCount);
        if (inputSamplesRead == 0)
            return 0;

        _inputCopy.SetBuffer(buffer, offset, sampleCount);

        var samplesRead = _ap[3].Read(buffer, offset, sampleCount);

        for (var i = 0; i < sampleCount; ++i)
            buffer[offset + i] = Wet * buffer[offset + i] + (1f - Wet) * _inputCopy.GetBuffer()[i];

        return samplesRead;
    }

    public override float Apply(float sample)
    {
        throw new NotSupportedException();
    }
}
