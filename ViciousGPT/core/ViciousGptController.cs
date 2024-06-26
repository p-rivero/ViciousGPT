using System.Diagnostics;
using System.IO;
using ViciousGPT.ApiClient;
using ViciousGPT.AudioProcessing;
using ViciousGPT.Properties;

namespace ViciousGPT;

internal class ViciousGptController
{
    public static bool OutputLogFile => Settings.Default.OutputLogFile;
    public static bool OutputIntermediaryResults => Settings.Default.OutputIntermediaryResults;

    public string UserInputLanguageIso => Settings.Default.UserInputLanguage;

    public float SynthesizedVoiceSpeedChange { get; set; } = 0.85f;

    public static string UserCharatcterName => Settings.Default.CharacterName;

    public static List<int> SelectedActs {
        get
        {
            List<int> result = [];
            if (Settings.Default.SelectedAct1) result.Add(1);
            if (Settings.Default.SelectedAct2) result.Add(2);
            if (Settings.Default.SelectedAct3) result.Add(3);
            return result;
        }
    }

    private readonly MicrophoneRecorder recorder = new();
    private readonly AudioTrimmer trimmer = new();
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
        string transcript = await MeasureTime(() => openAiClient.TranscribeAudio(audioData, UserInputLanguageIso), "Transcribe");
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
        Trace.TraceInformation($"Trimmed file is {percentReduction}% smaller than original file.");
        return trimmedAudio;
    }

    private async Task<byte[]> SynthesizeWithEffects(string text)
    {
        byte[] rawAudio = await MeasureTime(() => textToSpeech.Synthesize(text), "Synthesize");
        OutputAudio(rawAudio, "rawResponse.wav");

        byte[] slowedAudio = MeasureTime(() => audioSpeed.ChangeSpeed(rawAudio, SynthesizedVoiceSpeedChange), "ChangeSpeed");
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
        Trace.TraceInformation($"{actionName} took {stopWatch.Elapsed.TotalMilliseconds} ms");
        return result;
    }
    
    private static T MeasureTime<T>(Func<T> action, string actionName)
    {
        var stopWatch = Stopwatch.StartNew();
        T result = action();
        Trace.TraceInformation($"{actionName} took {stopWatch.Elapsed.TotalMilliseconds} ms");
        return result;
    }

    private void OutputAudio(byte[] audioData, string fileName)
    {
        if (OutputIntermediaryResults)
        {
            Trace.TraceInformation($"Outputting file {fileName}");
            File.WriteAllBytes(fileName, audioData);
        }
    }

    private void OutputText(string text, string title)
    {
        Trace.TraceInformation($"{title.ToUpper()}:");
        Trace.TraceInformation(text);
        Trace.TraceInformation("=====================================");
        if (OutputIntermediaryResults)
        {
            File.WriteAllText($"{title}.txt", text);
        }
    }
    
    private async Task<string> GenerateResponse(string input)
    {
        string systemPrompt = SystemPrompt.GetSystemPrompt(UserCharatcterName, UserInputLanguageIso, SelectedActs);
        return await openAiClient.CompletePrompt(systemPrompt, input);
    }
}
