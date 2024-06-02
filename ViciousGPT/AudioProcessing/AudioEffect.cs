using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace ViciousGPT.AudioProcessing;

internal abstract class AudioEffect
{
    protected static byte[] ToByteArray(ISampleProvider sampleProvider)
    {
        using var outputStream = new MemoryStream();
        SampleToWaveProvider16 trimmedWaveProvider = new(sampleProvider);
        WaveFileWriter.WriteWavFileToStream(outputStream, trimmedWaveProvider);
        return outputStream.ToArray();
    }

    protected static byte[] ToByteArray(float[] samples, int samplesCount, WaveFormat waveFormat)
    {
        using var outputStream = new MemoryStream();
        using WaveFileWriter writer = new WaveFileWriter(outputStream, waveFormat);
        return outputStream.ToArray();
    }
}
