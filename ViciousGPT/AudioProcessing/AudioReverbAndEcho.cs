using NAudio.Wave;

namespace ViciousGPT.AudioProcessing;

internal class AudioReverbAndEcho : AudioEffect
{
    public TimeSpan EchoDelay { get; set; } = TimeSpan.FromMilliseconds(500);
    public float EchoDecayFactor { get; set; } = 0.75f;
    public float ReverbRoomSize { get; set; } = 0.5f;

    public byte[] ApplyReverbAndEcho(byte[] inputAudio)
    {
        using MemoryStream memoryStream = new(inputAudio);
        using WaveFileReader inputStream = new(memoryStream);
        ISampleProvider sampleProvider = inputStream.ToSampleProvider();

        sampleProvider = new EchoEffect(sampleProvider, EchoDelay.TotalSeconds, EchoDecayFactor);
        //sampleProvider = new ReverbEffect(sampleProvider, ReverbRoomSize);
        return ToByteArray(sampleProvider);
    }
}

public class EchoEffect : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float[] delayRingBuffer;
    private int delayBufferIndex = 0;
    private readonly float delayVolumeAmount;

    public EchoEffect(ISampleProvider source, double delaySeconds, float decay)
    {
        this.source = source;
        int delaySamples = (int)(source.WaveFormat.SampleRate * delaySeconds);
        delayRingBuffer = new float[delaySamples];
        delayVolumeAmount = 1 - decay;
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

        return samplesRead;
    }
}

public class ReverbEffect : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly Freeverb reverb;

    public ReverbEffect(ISampleProvider source, float roomSize)
    {
        this.source = source;
        reverb = new Freeverb(source.WaveFormat.SampleRate)
        {
            RoomSize = roomSize
        };
    }

    public WaveFormat WaveFormat => source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);
        float[] inputBuffer = new float[samplesRead];
        Array.Copy(buffer, offset, inputBuffer, 0, samplesRead);

        float[] outputBufferLeft = new float[samplesRead];
        float[] outputBufferRight = new float[samplesRead];

        reverb.Process(inputBuffer, outputBufferLeft, outputBufferRight);

        for (int i = 0; i < samplesRead; i++)
        {
            buffer[offset + i] = outputBufferLeft[i];
        }

        return samplesRead;
    }
}
