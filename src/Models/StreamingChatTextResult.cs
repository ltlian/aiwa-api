using System.Text;
using Lli.OpenAi.Core.Schema.Chat;
using Microsoft.AspNetCore.Mvc;

namespace AIWA.API.Models;

public class StreamingChatTextResult(IAsyncEnumerable<CreateChatCompletionStreamResponse> chatCompletionResponse) : IActionResult
{
    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = "text/plain";

        await foreach (var data in chatCompletionResponse.WithCancellation(context.HttpContext.RequestAborted))
        {
            if (data.Choices[0].Delta.Content is string content)
            {
                var memory = context.HttpContext.Response.BodyWriter.GetMemory(Encoding.UTF8.GetByteCount(content));
                int bytesWritten = Encoding.UTF8.GetBytes(content, memory.Span);
                context.HttpContext.Response.BodyWriter.Advance(bytesWritten);
                var result = await context.HttpContext.Response.BodyWriter.FlushAsync(context.HttpContext.RequestAborted);
                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
    }
}
