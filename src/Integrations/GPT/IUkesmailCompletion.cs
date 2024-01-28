using Lli.OpenAi.Core.Schema.Chat;

namespace AIWA.API.Integrations.GPT;

public interface IUkesmailCompletion
{
    Task<CreateChatCompletionResponse> GenerateUkesMailAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default);
    IAsyncEnumerable<CreateChatCompletionStreamResponse> GetUkesMailStreamingAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default);
}
