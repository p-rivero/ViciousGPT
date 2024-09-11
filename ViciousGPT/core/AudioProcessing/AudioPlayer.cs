using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ViciousGPT.AudioProcessing;

class AudioPlayer : IDisposable
{
    private readonly WaveOutEvent waveOut = new();

    public async Task Play(byte[] audioData)
    {
        using MemoryStream stream = new(audioData);
        using WaveFileReader reader = new(stream);
        await PlayWav(reader);
    }
    
    public async Task Play(UnmanagedMemoryStream audioData)
    {
        using AudioFileReader reader = Extensions.GetAudioFileReaderFromStream(audioData);
        await PlayWav(reader);
    }

    private async Task PlayWav(WaveStream reader)
    {
        waveOut.Init(reader);
        waveOut.Play();
        while (waveOut.PlaybackState == PlaybackState.Playing)
        {
            await Task.Delay(100);
        }
    }

    public void Stop()
    {
        waveOut.Stop();
    }


    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            waveOut.Dispose();
        }
    }
}


// Adapted from https://stackoverflow.com/questions/49178123/how-to-play-wav-file-from-embedded-resources-with-naudio
public static class Extensions
{
    public static AudioFileReader GetAudioFileReaderFromStream(Stream stream)
    {
        AudioFileReader reader = (AudioFileReader)RuntimeHelpers.GetUninitializedObject(typeof(AudioFileReader));

        Type type = reader.GetType();
        type.GetField("lockObject", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(reader, new object());
        var readerStream = GetWaveStream(stream);
        type.GetField("readerStream", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(reader, readerStream);
        var sbps = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
        type.GetField("sourceBytesPerSample", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(reader, sbps);
        var sampleChannel = new SampleChannel(readerStream, forceStereo: false);
        type.GetField("sampleChannel", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(reader, sampleChannel);
        var dbps = 4 * sampleChannel.WaveFormat.Channels;
        type.GetField("destBytesPerSample", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(reader, dbps);
        var std = dbps * (readerStream.Length / sbps);
        type.GetField("length", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(reader, std);

        return reader;
    }

    private static WaveStream GetWaveStream(Stream stream)
    {
        WaveStream readerStream = new WaveFileReader(stream);
        if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
        {
            readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
            readerStream = new BlockAlignReductionStream(readerStream);
        }
        return readerStream;
    }
}