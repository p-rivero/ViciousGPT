using Google.Cloud.TextToSpeech.V1;

namespace ViciousGPT.ApiClient;

internal class TextToSpeech : GoogleApiClient
{
    private readonly TextToSpeechClient speechClient;

    public TextToSpeech()
    {
        speechClient = TextToSpeechClient.Create();
    }

    public async Task<byte[]> Synthesize(string text)
    {
        SynthesisInput input = new() { Text = text };

        VoiceSelectionParams voice = new()
        {
            LanguageCode = "en-US",
            Name = "en-US-Journey-O"
        };

        AudioConfig audioConfig = new() { AudioEncoding = AudioEncoding.Linear16 };

        SynthesizeSpeechResponse response = await speechClient.SynthesizeSpeechAsync(input, voice, audioConfig);

        return response.AudioContent.ToByteArray();
    }
}
