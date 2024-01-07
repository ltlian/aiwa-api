using Azure.AI.OpenAI;
using Lli.OpenAi.Core.Client;

namespace AIWA.API.Integrations.GPT
{
    public interface IChatCompletion
    {
        OpenAiHttpClient OpenAiHttpClient { get; }

        Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> GetChatCompletionStreamAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
