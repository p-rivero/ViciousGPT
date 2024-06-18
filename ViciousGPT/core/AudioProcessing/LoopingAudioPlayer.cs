using NAudio.Wave;
using System.IO;

namespace ViciousGPT.AudioProcessing;

public class LoopingAudioPlayer : IDisposable
{
    private readonly WaveOutEvent waveOutDevice = new();
    private WaveChannel32? volumeStream;

    public void Start(WaveStream audio)
    {
        Stop();
        LoopStream stream = new(audio);
        volumeStream = new WaveChannel32(stream) { Volume = 0 };
        waveOutDevice.Init(volumeStream);
        waveOutDevice.Play();
    }

    public void Start(UnmanagedMemoryStream audio)
    {
        Start(Extensions.GetAudioFileReaderFromStream(audio));
    }

    public void Stop()
    {
        waveOutDevice.Stop();
        volumeStream?.Dispose();
    }

    public async Task FadeIn(int fadeTimeMs)
    {
        await Fade(0, 1, fadeTimeMs);
    }

    public async Task FadeOut(int fadeTimeMs)
    {
        await Fade(1, 0, fadeTimeMs);
    }

    private async Task Fade(float startVolume, float endVolume, int fadeTimeMs)
    {
        const int STEPS = 50;
        if (volumeStream == null)
        {
            return;
        }

        float step = (endVolume - startVolume) / STEPS;
        int delay = fadeTimeMs / STEPS;
        for (int i = 0; i <= STEPS; i++)
        {
            volumeStream.Volume = startVolume + (step * i);
            await Task.Delay(delay);
        }
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
            waveOutDevice.Dispose();
            volumeStream?.Dispose();
        }
    }
}


// Adapted from https://mark-dot-net.blogspot.com/2009/10/looped-playback-in-net-with-naudio.html
public class LoopStream(WaveStream sourceStream) : WaveStream
{
    private readonly WaveStream sourceStream = sourceStream;

    public bool EnableLooping { get; set; } = true;

    public override WaveFormat WaveFormat
    {
        get { return sourceStream.WaveFormat; }
    }
    public override long Length
    {
        get { return sourceStream.Length; }
    }

    public override long Position
    {
        get { return sourceStream.Position; }
        set { sourceStream.Position = value; }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                if (sourceStream.Position == 0 || !EnableLooping)
                {
                    break;
                }
                sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            sourceStream.Dispose();
        }
        base.Dispose(disposing);
    }
}
