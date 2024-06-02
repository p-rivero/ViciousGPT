using ViciousGPT.ApiClient;
using ViciousGPT.AudioProcessing;

namespace ViciousGPT;

internal class ViciousGptController
{
    public bool OutputIntermediaryResults { get; set; } = false;

    private readonly MicrophoneRecorder recorder = new();
    private readonly AudioTrimmer trimmer = new();
    private readonly SpeechToText speechToText = new();
    private readonly TextToSpeech textToSpeech = new();
    private readonly AudioReverbAndEcho audioReverbAndEcho = new();
    private readonly AudioSpeed audioSpeed = new();

    public void StartRecording()
    {
        recorder.Start();
    }

    public void CancelRecording()
    {
        recorder.Stop();
    }

    public async Task<byte[]> AcceptRecording()
    {
        byte[] audioData = recorder.Stop();
        OutputAudio(audioData, "microphoneInput.wav");
        byte[] trimmedAudio = trimmer.TrimSilence(audioData);
        OutputAudio(trimmedAudio, "trimmedMicrophoneInput.wav");

        float percentReduction = (1 - (float)trimmedAudio.Length / audioData.Length) * 100;
        Console.WriteLine($"Trimmed file is {percentReduction} shorter than original file.");

        string transcript = await speechToText.RecognizeSpeech(audioData);
        OutputText(transcript, "input_transcript");

        string response = await GenerateResponse(transcript);
        OutputText(response, "response");

        byte[] responseAudio = await textToSpeech.Synthesize(response);
        OutputAudio(responseAudio, "rawResponse.wav");

        byte[] slowResponseAudio = audioSpeed.ChangeSpeed(responseAudio, 0.8f);
        OutputAudio(slowResponseAudio, "slowedResponse.wav");

        byte[] processedResponseAudio = audioReverbAndEcho.ApplyReverbAndEcho(slowResponseAudio);
        OutputAudio(processedResponseAudio, "processedResponse.wav");

        return processedResponseAudio;
    }

    private void OutputAudio(byte[] audioData, string fileName)
    {
        if (OutputIntermediaryResults)
        {
            Console.WriteLine($"Outputting file {fileName}");
            File.WriteAllBytes(fileName, audioData);
        }
    }

    private void OutputText(string text, string title)
    {
        Console.WriteLine($"{title.ToUpper()}:");
        Console.WriteLine(text);
        Console.WriteLine("=====================================");
        if (OutputIntermediaryResults)
        {
            File.WriteAllText($"{title}.txt", text);
        }
    }

    private async Task<string> GenerateResponse(string input)
    {
        return await Task.FromResult("This will be a response in the future");
    }
}
