using NAudio.Wave;
using System;
using SoundTouch;
using NAudio.Wave.SampleProviders;

namespace ViciousGPT.AudioProcessing;

internal class AudioSpeed : AudioEffect
{
    public byte[] ChangeSpeed(byte[] audioData, float speed)
    {
        if (speed <= 0)
        {
            throw new ArgumentException("Speed must be greater than 0", nameof(speed));
        }

        using var inputStream = new MemoryStream(audioData);
        using var reader = new WaveFileReader(inputStream);
        SoundTouchProcessor soundTouch = new()
        {
            SampleRate = reader.WaveFormat.SampleRate,
            Channels = reader.WaveFormat.Channels,
            Tempo = speed
        };

        int channels = reader.WaveFormat.Channels;
        var inputBuffer = new float[reader.SampleCount * channels];
        int inputLength = reader.ToSampleProvider().Read(inputBuffer, 0, inputBuffer.Length);

        var outputBuffer = new float[inputBuffer.Length];
        soundTouch.PutSamples(inputBuffer, inputLength / channels);
        int outputSamples = soundTouch.ReceiveSamples(outputBuffer, outputBuffer.Length / channels);

        return ToByteArray(outputBuffer, outputSamples, reader.WaveFormat);
    }

}
