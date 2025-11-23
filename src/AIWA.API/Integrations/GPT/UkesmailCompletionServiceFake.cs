using Lli.OpenAi.Core.Schema;
using Lli.OpenAi.Core.Schema.Chat;

namespace AIWA.API.Integrations.GPT;

public class UkesmailCompletionServiceFake() : IUkesmailCompletion
{
    private const int Iterations = 100;
    private const int DelayMs = 500;

    public Task<CreateChatCompletionResponse> GenerateUkesMailAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = GetChatCompletionsOptions(string.Join(", ", promptExtras), 1);
        return Task.FromResult(MockOpenAiTextGenerator.GenerateChatCompletionsResponse(chatCompletionsOptions, Iterations));
    }

    public IAsyncEnumerable<CreateChatCompletionStreamResponse> GetUkesMailStreamingAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = GetChatCompletionsOptions(string.Join(", ", promptExtras), 1);
        return MockOpenAiTextGenerator.GenerateChatCompletionsStreamResponseAsync(chatCompletionsOptions, Iterations, DelayMs, cancellationToken);
    }

    private static CreateChatCompletionRequest GetChatCompletionsOptions(string prompt, int choiceCount = 1, bool stream = false)
    {
        List<IChatCompletionRequestMessage> messages = [new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartText(prompt)])];
        return new(messages, OpenAIModel.Gpt35Turbo, MaxTokens: 5000, N: choiceCount, Stream: stream);
    }
}
