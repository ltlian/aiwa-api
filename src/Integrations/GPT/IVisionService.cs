using Azure.AI.OpenAI;

namespace AIWA.API.Integrations.GPT
{
    public interface IVisionService
    {
        Task<string> GetChatCompletionAsync(string threadId, string imageUrl, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string?> GetChatCompletionStreamAsync(string threadId, string imageUrl, CancellationToken cancellationToken = default);
    }
}
