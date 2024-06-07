using Google.Cloud.Speech.V2;
using Google.Protobuf;
using System.Text;

namespace ViciousGPT.ApiClient;

internal class SpeechToText : GoogleApiClient
{
    private readonly Lazy<SpeechClient> speechClient = new(GetClient);

    public async Task<string> RecognizeSpeech(byte[] audioData, string isoLanguageCode)
    {
        ByteString audioBytes = ByteString.CopyFrom(audioData);

        RecognitionConfig recognitionConfig = new()
        {
            Model = "chirp_2",
            LanguageCodes = { isoLanguageCode },
            AutoDecodingConfig = new AutoDetectDecodingConfig()
        };

        string implicitRecognizer = "_";
        RecognizeRequest request = new()
        {
            Config = recognitionConfig,
            Recognizer = implicitRecognizer,
            Content = audioBytes
        };

        RecognizeResponse response = await speechClient.Value.RecognizeAsync(request);

        StringBuilder transcriptBuilder = new();
        foreach (SpeechRecognitionResult result in response.Results)
        {
            if (result.Alternatives.Count > 1)
            {
                var mostLikelyAlternative = result.Alternatives[0];
                transcriptBuilder.Append(mostLikelyAlternative.Transcript);
            }
        }
        return transcriptBuilder.ToString();
    }
    private static SpeechClient GetClient()
    {
        SetCredentialsEnv();
        return SpeechClient.Create();
    }
}
