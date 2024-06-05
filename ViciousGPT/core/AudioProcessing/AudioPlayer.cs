using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViciousGPT.AudioProcessing;

class AudioPlayer : IDisposable
{
    private readonly WaveOutEvent waveOut = new();

    public async Task Play(byte[] audioData)
    {
        using MemoryStream stream = new(audioData);
        using WaveFileReader reader = new(stream);
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
        waveOut.Dispose();
    }
}
