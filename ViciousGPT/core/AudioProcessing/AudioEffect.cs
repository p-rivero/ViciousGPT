using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.IO;

namespace ViciousGPT.AudioProcessing;

internal abstract class AudioEffect
{
    protected static byte[] ToByteArray(ISampleProvider sampleProvider)
    {
        using MemoryStream outputStream = new();
        SampleToWaveProvider16 trimmedWaveProvider = new(sampleProvider);
        WaveFileWriter.WriteWavFileToStream(outputStream, trimmedWaveProvider);
        return outputStream.ToArray();
    }

    protected static byte[] ToByteArray(float[] samples, int samplesCount, WaveFormat waveFormat)
    {
        using MemoryStream outputStream = new();
        using WaveFileWriter writer = new(outputStream, waveFormat);
        writer.WriteSamples(samples, 0, samplesCount);
        writer.Flush();
        return outputStream.ToArray();
    }
}
