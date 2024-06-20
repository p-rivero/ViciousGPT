using NAudio.Wave;
using System.IO;

namespace ViciousGPT.AudioProcessing;

public class MicrophoneRecorder
{
    private static readonly WaveFormat RECORDING_FORMAT = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1); // 44.1kHz, Mono

    private WaveInEvent? waveIn;
    private MemoryStream? memoryStream;
    private WaveFileWriter? waveFileWriter;

    public void Start()
    {
        Initialize();
        waveIn!.StartRecording();
    }

    public byte[] Stop()
    {
        if (waveIn == null || memoryStream == null || waveFileWriter == null)
        {
            throw new InvalidOperationException("Called MicrophoneRecorder.Stop() without calling Start() first");
        }

        waveIn.StopRecording();
        waveIn.Dispose();

        waveFileWriter.Flush();
        waveFileWriter.Close();
        waveFileWriter.Dispose();

        byte[] audioData = memoryStream!.ToArray();
        memoryStream.Dispose();

        return audioData;
    }

    private void Initialize()
    {
        waveIn = new() { WaveFormat = RECORDING_FORMAT };
        memoryStream = new();
        waveFileWriter = new WaveFileWriter(memoryStream, RECORDING_FORMAT);
        waveIn.DataAvailable += OnDataAvailable;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        waveFileWriter!.Write(e.Buffer, 0, e.BytesRecorded);
    }
}
