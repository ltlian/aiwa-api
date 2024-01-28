using System.Runtime.CompilerServices;
using AIWA.API.Data.Models;
using Lli.OpenAi.Core.Schema.Chat;
using Microsoft.Extensions.Caching.Memory;

namespace AIWA.API.Data;

/*
public class InMemoryStore : IDataStore
{
    public IMemoryCache Cache { get; } = new MemoryCache
    (
        new MemoryCacheOptions()
    );

    public Task<List<IChatCompletionRequestMessage>> AddMessageToThreadAsync(int userId, string threadId, IChatCompletionRequestMessage message)
    {
        var key = $"user/{userId}/threads/{threadId}/messages";

        var cls = Cache.GetOrCreate(key, ce =>
        {
            return ce.Value is List<IChatCompletionRequestMessage> ls ? ls : ([]);
        })!;

        cls.Add(message);
        IChatCompletionRequestMessage[] arr = new IChatCompletionRequestMessage[cls.Count];
        cls.CopyTo(arr);

        return Task.FromResult(arr.ToList());
    }

    public Task AddMessageToThreadAsync(int userId, string threadId, string role, string content, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<InteractionUnit?> GetLastThreadMessageOrDefaultAsync(int userId, string threadId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<InteractionFlow> GetOrCreateUserThreadAsync(int userId, string threadId, CancellationToken cancellationToken = default)
    {
        var key = $"user/{userId}/threads/{threadId}";

        return Task.FromResult(Cache.GetOrCreate(key, ce =>
        {
            if (ce.Value is InteractionFlow imageConversationThread)
            {
                return imageConversationThread;
            }
            else
            {
                var thread = new InteractionFlow { CreatedAt = DateTimeOffset.UtcNow, Id = threadId };
                ce.SetValue(thread);
                return thread;
            }
        })!);
    }

    public async IAsyncEnumerable<InteractionUnit> GetThreadMessagesAsync(int userId, string threadId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var key = $"user/{userId}/threads";

        var r = Cache.GetOrCreate(key, ce =>
        {
            return ce.Value is List<InteractionUnit> userThreads ? userThreads : [];
        })!;

        foreach (var userThread in r)
        {
            yield return await Task.FromResult(userThread);
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<InteractionFlow> GetUserThreadsAsync(int userId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        var key = $"user/{userId}/threads";

        var threads = Cache.Get<List<InteractionFlow>>(key);
        if (threads != null)
        {
            foreach (var thread in threads)
            {
                yield return thread;
            }
        }
        else
        {
            yield break;
        }
    }
}
/**/