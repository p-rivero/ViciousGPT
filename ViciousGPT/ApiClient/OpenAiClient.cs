using OpenAI.Managers;
using OpenAI;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using static OpenAI.ObjectModels.SharedModels.IOpenAiModels;

namespace ViciousGPT.ApiClient;

internal class OpenAiClient : ApiClient
{
    private const string OPENAI_API_KEY_FILE = "openai-key.txt";

    private static readonly string MODEL = Models.Gpt_3_5_Turbo;

    // Higher is more random
    private const float TEMPERATURE = 0.7f;

    private readonly OpenAIService openAiService;

    public OpenAiClient()
    {
        OpenAiOptions options = new() { ApiKey = GetKey(OPENAI_API_KEY_FILE) };
        openAiService = new(options);
    }

    public async Task<string> CompletePrompt(string systemPrompt, string userPrompt)
    {
        ChatCompletionCreateRequest request = new()
        {
            Model = MODEL,
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
        var response = await openAiService.ChatCompletion.CreateCompletion(request);
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

}
