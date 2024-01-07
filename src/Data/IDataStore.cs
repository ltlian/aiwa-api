using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Thread;

namespace AIWA.API.Data
{
    public interface IDataStore
    {
        IAsyncEnumerable<ThreadObject> GetUserThreadsAsync(int userId, CancellationToken cancellationToken);
        Task<ThreadObject> GetOrCreateUserThreadAsync(int userId, ThreadObject thread);
        Task<List<IChatCompletionRequestMessage>> AddMessageToThreadAsync(int userId, string threadId, IChatCompletionRequestMessage message);
    }
}