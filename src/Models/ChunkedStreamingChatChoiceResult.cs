using System.Text;

using Azure.AI.OpenAI;

using Microsoft.AspNetCore.Mvc;

namespace AIWA.API.Models;

public class ChunkedStreamingChatChoiceResult(IAsyncEnumerable<StreamingChatChoice> streamingChatChoice) : IActionResult
{
    private readonly IAsyncEnumerable<StreamingChatChoice> _streamingChatChoice = streamingChatChoice;
    private const string ContentType = "text/plain";

    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = ContentType;
        await foreach (var data in _streamingChatChoice.WithCancellation(context.HttpContext.RequestAborted))
        {
            await foreach (var item in data.GetMessageStreaming().WithCancellation(context.HttpContext.RequestAborted))
            {
                if (data.FinishReason.Equals(CompletionsFinishReason.Stopped))
                {
                    break;
                }

                var memory = context.HttpContext.Response.BodyWriter.GetMemory(Encoding.UTF8.GetByteCount(item.Content));
                int bytesWritten = Encoding.UTF8.GetBytes(item.Content, memory.Span);
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
