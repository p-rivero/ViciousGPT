using NAudio.Wave;
using System.IO;

namespace ViciousGPT.AudioProcessing;

public class MicrophoneRecorder
{
    private static readonly WaveFormat RECORDING_FORMAT = new(44100, 1); // 44.1kHz, Mono

    private readonly WaveInEvent waveIn = new() { WaveFormat = RECORDING_FORMAT };
    private readonly MemoryStream memoryStream = new();
    private readonly WaveFileWriter waveFileWriter;

    public MicrophoneRecorder()
    {
        waveFileWriter = new WaveFileWriter(memoryStream, RECORDING_FORMAT);
        waveIn.DataAvailable += OnDataAvailable;
    }

    public void Start()
    {
        waveIn.StartRecording();
    }

    public byte[] Stop()
    {
        waveIn.StopRecording();
        waveIn.Dispose();

        waveFileWriter.Flush();
        waveFileWriter.Close();
        waveFileWriter.Dispose();

        byte[] audioData = memoryStream.ToArray();
        memoryStream.Dispose();

        return audioData;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
    }
}
