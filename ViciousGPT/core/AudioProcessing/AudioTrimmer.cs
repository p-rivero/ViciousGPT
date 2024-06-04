using NAudio.Wave;

namespace ViciousGPT.AudioProcessing;

internal class AudioTrimmer : AudioEffect
{
    public TimeSpan SilenceDuration { get; set; } = TimeSpan.FromMilliseconds(300);

    public float SilenceThresholdDb { get; set; } = -40f;

    public byte[] TrimSilence(byte[] audioData)
    {
        using MemoryStream inputStream = new(audioData);
        using WaveFileReader waveReader = new(inputStream);
        ISampleProvider sampleProvider = waveReader.ToSampleProvider();
        ISampleProvider trimmedAudio = RemoveSilence(sampleProvider, waveReader.WaveFormat);
        return ToByteArray(trimmedAudio);
    }

    private ISampleProvider RemoveSilence(ISampleProvider sampleProvider, WaveFormat waveFormat)
    {
        float threshold = (float)Math.Pow(10, SilenceThresholdDb / 20.0);
        int silenceDuration = (int)(waveFormat.SampleRate * SilenceDuration.TotalSeconds);
        float[] buffer = new float[waveFormat.SampleRate * waveFormat.Channels];
        List<float> sampleBuffer = [];

        int samplesRead;
        bool inSilence = true;
        int silenceSamples = 0;

        while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                if (Math.Abs(buffer[i]) < threshold)
                {
                    silenceSamples++;
                    if (silenceSamples > silenceDuration && inSilence)
                    {
                        silenceSamples = 0;
                    }
                    else if (silenceSamples <= silenceDuration)
                    {
                        sampleBuffer.Add(buffer[i]);
                    }
                }
                else
                {
                    inSilence = false;
                    silenceSamples = 0;
                    sampleBuffer.Add(buffer[i]);
                }
            }
        }

        return new ListSampleProvider(sampleBuffer.ToArray(), waveFormat);
    }

    private sealed class ListSampleProvider(float[] samples, WaveFormat waveFormat) : ISampleProvider
    {
        private readonly float[] samples = samples;
        private int position;
        public WaveFormat WaveFormat { get; } = waveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int availableSamples = samples.Length - position;
            int samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(samples, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return samplesToCopy;
        }
    }
}
