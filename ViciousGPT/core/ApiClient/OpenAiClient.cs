using OpenAI.Managers;
using OpenAI;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using ViciousGPT.Properties;

namespace ViciousGPT.ApiClient;

internal class OpenAiClient
{
    // Higher is more random
    private const float TEMPERATURE = 0.7f;

    private readonly Lazy<OpenAIService> openAiService = new(GetService);

    public async Task<string> CompletePrompt(string systemPrompt, string userPrompt)
    {
        ChatCompletionCreateRequest request = new()
        {
            Model = Models.Gpt_3_5_Turbo,
            Temperature = TEMPERATURE,
            Messages = new List<ChatMessage>
            {
                new() {
                    Role = "system",
                    Content = systemPrompt
                },
                new()
                {
                    Role = "user",
                    Content = userPrompt
                }
            }
        };
        var response = await openAiService.Value.ChatCompletion.CreateCompletion(request);
        if (!response.Successful)
        {
            throw new ArgumentException("Failed to complete prompt. Error: " + response.Error?.ToString());
        }
        if (response.Choices.Count == 0)
        {
            throw new ArgumentException("Failed to complete prompt. No choices returned.");
        }
        return response.Choices[0].Message.Content ?? throw new ArgumentException("Failed to complete prompt. No content returned.");
    }
    
    public async Task<string> TranscribeAudio(byte[] audioData, string isoLanguageCode)
    {
        AudioCreateTranscriptionRequest request = new()
        {
            Model = Models.WhisperV1,
            File = audioData,
            FileName = "audio.wav",
            Language = isoLanguageCode,
            //Prompt = prompt, // TODO: see if this is needed
            ResponseFormat = "text"
        };
        var response = await openAiService.Value.CreateTranscription(request);
        if (!response.Successful)
        {
            throw new ArgumentException("Failed to transcribe audio. Error: " + response.Error?.ToString());
        }
        return response.Text;
    }

    private static OpenAIService GetService()
    {
        OpenAiOptions options = new() { ApiKey = Settings.Default.OpenAiKey };
        return new OpenAIService(options);
    }

}
