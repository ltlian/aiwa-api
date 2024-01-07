using System.Runtime.CompilerServices;
using Lli.OpenAi.Core.Schema.Chat;
using Lli.OpenAi.Core.Schema.Thread;
using Microsoft.Extensions.Caching.Memory;

namespace AIWA.API.Data
{
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
                if (ce.Value is List<IChatCompletionRequestMessage> ls)
                {
                    return ls;
                    //IChatCompletionRequestMessage[] arr = new IChatCompletionRequestMessage[ls.Count];
                    //ls.CopyTo(arr);
                    //ls.Add(message);
                    //ce.SetValue(ls);
                    //return [.. ls];
                }
                else
                {
                    return [];
                    //var newList = new List<IChatCompletionRequestMessage> { message };
                    //ce.SetValue(newList);
                    //return newList;
                }
            })!;

            //cls.Add(message);

            cls.Add(message);
            IChatCompletionRequestMessage[] arr = new IChatCompletionRequestMessage[cls.Count];
            cls.CopyTo(arr);
            

            return Task.FromResult(arr.ToList());
        }

        public Task<ThreadObject> GetOrCreateUserThreadAsync(int userId, ThreadObject thread)
        {
            var key = $"user/{userId}/threads/{thread.Id}";

            return Task.FromResult(Cache.GetOrCreate(key, ce =>
            {
                if (ce.Value is ThreadObject threadObject)
                {
                    return threadObject;
                }
                else
                {
                    ce.SetValue(thread);
                    return thread;
                }
            })!);
        }

        public async IAsyncEnumerable<ThreadObject> GetUserThreadsAsync(int userId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var key = $"user/{userId}/threads";

            var r = Cache.GetOrCreate(key, ce =>
            {
                if (ce.Value is List<ThreadObject> userThreads)
                {
                    return userThreads;
                }
                else
                {
                    return [];
                }
            })!;

            foreach (var userThread in r)
            {
                yield return await Task.FromResult(userThread);
            }
        }
    }
}