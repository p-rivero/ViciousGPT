using System.Diagnostics;
using System.IO;
using ViciousGPT.ApiClient;
using ViciousGPT.AudioProcessing;
using ViciousGPT.Properties;

namespace ViciousGPT;

internal class ViciousGptController
{
    public bool OutputIntermediaryResults { get; set; } = true;

    public string UserInputLanguage { get; set; } = "es-ES";
    public static string UserCharatcterName => Settings.Default.CharacterName;

    private readonly MicrophoneRecorder recorder = new();
    private readonly AudioTrimmer trimmer = new();
    private readonly SpeechToText speechToText = new();
    private readonly OpenAiClient openAiClient = new();
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

    public async Task<(string, byte[])> AcceptRecording()
    {
        string input = await GetUserInput();
        string response = await GetResponse(input);
        return (response, await SynthesizeWithEffects(response));
    }

    private async Task<string> GetUserInput()
    {
        byte[] audioData = GetTrimmedAudioInput();
        string transcript = await MeasureTime(() => speechToText.RecognizeSpeech(audioData, UserInputLanguage), "Transcribe");
        OutputText(transcript, "input_transcript");
        return transcript;
    }

    private byte[] GetTrimmedAudioInput()
    {
        byte[] audioData = recorder.Stop();
        OutputAudio(audioData, "microphoneInput.wav");

        byte[] trimmedAudio = MeasureTime(() => trimmer.TrimSilence(audioData), "TrimSilence");
        OutputAudio(trimmedAudio, "trimmedMicrophoneInput.wav");
        float percentReduction = (1 - (float)trimmedAudio.Length / audioData.Length) * 100;
        Console.WriteLine($"Trimmed file is {percentReduction}% shorter than original file.");
        return trimmedAudio;
    }

    private async Task<byte[]> SynthesizeWithEffects(string text)
    {
        byte[] rawAudio = await MeasureTime(() => textToSpeech.Synthesize(text), "Synthesize");
        OutputAudio(rawAudio, "rawResponse.wav");

        byte[] slowedAudio = MeasureTime(() => audioSpeed.ChangeSpeed(rawAudio, 0.8f), "ChangeSpeed");
        OutputAudio(slowedAudio, "slowedResponse.wav");

        byte[] processedAudio = MeasureTime(() => audioReverbAndEcho.ApplyReverbAndEcho(slowedAudio), "ApplyReverbAndEcho");
        OutputAudio(processedAudio, "processedResponse.wav");
        return processedAudio;
    }

    private async Task<string> GetResponse(string input)
    {
        string response = await MeasureTime(() => GenerateResponse(input), "GenerateResponse");
        OutputText(response, "response");
        return response;
    }

    private static async Task<T> MeasureTime<T>(Func<Task<T>> action, string actionName)
    {
        var stopWatch = Stopwatch.StartNew();
        T result = await action();
        Console.WriteLine($"{actionName} took {stopWatch.Elapsed.TotalMilliseconds} ms");
        return result;
    }
    
    private static T MeasureTime<T>(Func<T> action, string actionName)
    {
        var stopWatch = Stopwatch.StartNew();
        T result = action();
        Console.WriteLine($"{actionName} took {stopWatch.Elapsed.TotalMilliseconds} ms");
        return result;
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
        string systemPrompt = SystemPrompt.GetSystemPrompt(UserCharatcterName, UserInputLanguage);
        return await openAiClient.CompletePrompt(systemPrompt, input);
    }
}
