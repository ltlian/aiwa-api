using Lli.OpenAi.Core.Schema.Chat;

namespace AIWA.API.Integrations.GPT;

public interface IChatCompletion
{
    Task<CreateChatCompletionResponse> CreateChatCompletionAsync(string prompt, CancellationToken cancellationToken = default);
    IAsyncEnumerable<CreateChatCompletionStreamResponse> CreateChatCompletionStreamedAsync(string prompt, CancellationToken cancellationToken = default);
}
