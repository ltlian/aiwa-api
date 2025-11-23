using System.Runtime.CompilerServices;
using System.Text;

using Lli.OpenAi.Core.Schema;
using Lli.OpenAi.Core.Schema.Chat;

namespace AIWA.API.Integrations.GPT;

public static class MockOpenAiTextGenerator
{
    public static async IAsyncEnumerable<CreateChatCompletionStreamResponse> GenerateChatCompletionsStreamResponseAsync(
        CreateChatCompletionRequest chatCompletionsOptions,
        int iterations = 100,
        int delayMs = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var index = 0;
        var modelString = OpenAIModelMap.ToString(chatCompletionsOptions.Model);
        foreach (var chunk in MockTextGenerator.GenerateMockTextStream(iterations))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return new CreateChatCompletionStreamResponse(
               Guid.NewGuid().ToString(),
               [new(new ChatCompletionStreamResponseDelta("assistant", chunk), null, index++)],
               0,
               modelString,
               null,
               "object");

            if (delayMs > 0)
                await Task.Delay(Random.Shared.Next(delayMs / 10, delayMs), CancellationToken.None);
        }
    }

    public static CreateChatCompletionResponse GenerateChatCompletionsResponse(
        CreateChatCompletionRequest chatCompletionsOptions,
        int iterations = 100)
    {
        var modelString = OpenAIModelMap.ToString(chatCompletionsOptions.Model);
        var sb = new StringBuilder();

#if DEBUG
        var dsb = new StringBuilder();
        dsb.Append(MockTextGenerator.GenerateMockTextStream(iterations));
        Console.WriteLine(dsb.ToString());
#endif
        foreach (var chunk in MockTextGenerator.GenerateMockTextStream(iterations))
        {
            sb.Append(chunk);
        }

        return new CreateChatCompletionResponse(
               Guid.NewGuid().ToString(),
               [new(new(sb.ToString()), FinishReason.Stop, 0)],
               DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
               modelString,
               null,
               "object",
               null);
    }
}
