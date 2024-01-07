using Microsoft.AspNetCore.Mvc;

namespace AIWA.API.Models;

public class StreamingAudioResult(Stream stream) : IActionResult
{
    private const int BufferSize = 4096;

    public async Task ExecuteResultAsync(ActionContext context)
    {
        int bytesRead;
        byte[] buffer = new byte[BufferSize];
        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            await context.HttpContext.Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead));
            await context.HttpContext.Response.Body.FlushAsync();

            // Artificial delay
            // await Task.Delay(500);
        }
    }
}
