using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ViciousGPT.AudioProcessing;

internal class AudioReverbAndEcho : AudioEffect
{
    public TimeSpan EchoDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    public float EchoDecayFactor { get; set; } = 0.9f;

    public float ReverbWet { get; set; } = 0.15f;
    public float ReverbRoomSize { get; set; } = 0.7f;

    public byte[] ApplyReverbAndEcho(byte[] inputAudio)
    {
        using MemoryStream memoryStream = new(inputAudio);
        using WaveFileReader inputStream = new(memoryStream);
        ISampleProvider sampleProvider = inputStream.ToSampleProvider();

        sampleProvider = new EchoEffect(sampleProvider, EchoDelay.TotalSeconds, EchoDecayFactor);
        sampleProvider = new ReverbEffect(sampleProvider, ReverbWet, ReverbRoomSize);
        return ToByteArray(sampleProvider);
    }
}

public class EchoEffect : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float[] delayRingBuffer;
    private int delayBufferIndex = 0;
    private readonly float delayVolumeAmount;
    private int insertSamplesAfter;

    public EchoEffect(ISampleProvider source, double delaySeconds, float decay, float insertSecondsAfter = 0.5f)
    {
        this.source = source;
        int delaySamples = (int)(source.WaveFormat.SampleRate * delaySeconds);
        delayRingBuffer = new float[delaySamples];
        delayVolumeAmount = 1 - decay;
        insertSamplesAfter = (int)(source.WaveFormat.SampleRate * insertSecondsAfter);
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);

        for (int n = 0; n < samplesRead; n++)
        {
            float delayedSample = delayRingBuffer[delayBufferIndex];
            float newSample = buffer[n + offset] + delayedSample * delayVolumeAmount;
            buffer[n + offset] = newSample;

            // Store the sample in the delay buffer
            delayRingBuffer[delayBufferIndex] = buffer[n + offset];
            delayBufferIndex = (delayBufferIndex + 1) % delayRingBuffer.Length;
        }

        if (samplesRead == 0 && insertSamplesAfter > 0)
        {
            int samplesToInsert = Math.Min(count, insertSamplesAfter);
            for (int n = 0; n < samplesToInsert; n++)
            {
                float delayedSample = delayRingBuffer[delayBufferIndex];
                float newSample = 0 + delayedSample * delayVolumeAmount;
                buffer[n + offset] = newSample;

                // Store the sample in the delay buffer
                delayRingBuffer[delayBufferIndex] = 0;
                delayBufferIndex = (delayBufferIndex + 1) % delayRingBuffer.Length;
            }
            insertSamplesAfter -= samplesToInsert;
            return samplesToInsert;
        }
        return samplesRead;
    }
}

public class ReverbEffect : ISampleProvider
{
    private readonly ISampleProvider source;

    public ReverbEffect(ISampleProvider source, float wet, float roomSize)
    {
        ISampleProvider stereoSource = source.WaveFormat.Channels switch
        {
            1 => new MonoToStereoSampleProvider(source),
            2 => source,
            _ => throw new ArgumentException("Unsupported number of channels")
        };
        this.source = new Freeverb(stereoSource, roomSize, 0.3f)
        {
            Wet = wet
        };
    }

    public WaveFormat WaveFormat => source.WaveFormat; 
    public int Read(float[] buffer, int offset, int count)
    {
        return source.Read(buffer, offset, count);
    }
}
