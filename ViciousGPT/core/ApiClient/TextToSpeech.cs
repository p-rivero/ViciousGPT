using Google.Cloud.TextToSpeech.V1;
using ViciousGPT.Properties;

namespace ViciousGPT.ApiClient;

internal class TextToSpeech
{
    private readonly Lazy<TextToSpeechClient> speechClient = new(GetClient);

    public async Task<byte[]> Synthesize(string text)
    {
        SynthesisInput input = new() { Text = text };

        VoiceSelectionParams voice = new()
        {
            LanguageCode = "en-US",
            Name = "en-US-Journey-O"
        };

        AudioConfig audioConfig = new() { AudioEncoding = AudioEncoding.Linear16 };

        SynthesizeSpeechResponse response = await speechClient.Value.SynthesizeSpeechAsync(input, voice, audioConfig);

        return response.AudioContent.ToByteArray();
    }

    private static TextToSpeechClient GetClient()
    {
        string jsonKeyPath = Settings.Default.GoogleServiceAccountPath;
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonKeyPath);
        return TextToSpeechClient.Create();
    }
}
