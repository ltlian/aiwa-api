using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Schema.Chat;
using static Lli.OpenAi.Core.Schema.Models;

namespace AIWA.API.Integrations.GPT;

public class ChatCompletionService(IOpenAiHttpClient openAiHttpClient) : IChatCompletion
{
    public async Task<CreateChatCompletionResponse> CreateChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = GetChatCompletionsOptions(prompt, 1);
        var createChatCompletionResponse = await openAiHttpClient.CreateChatCompletionAsync(chatCompletionsOptions, cancellationToken);
        return createChatCompletionResponse ?? throw new NullReferenceException(nameof(createChatCompletionResponse));
    }

    public IAsyncEnumerable<CreateChatCompletionStreamResponse> CreateChatCompletionStreamedAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = GetChatCompletionsOptions(prompt, 1, true);
        return openAiHttpClient.CreateChatCompletionStreamedAsync(chatCompletionsOptions, cancellationToken);
    }

    private static CreateChatCompletionRequest GetChatCompletionsOptions(string prompt, int choiceCount = 1, bool stream = false)
    {
        List<IChatCompletionRequestMessage> messages = [new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartText(prompt)])];
        return new(messages, GPT35_turbo, MaxTokens: 5000, N: choiceCount, Stream: stream);
    }
}
