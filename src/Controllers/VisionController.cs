using System.Collections.Concurrent;
using AIWA.API.Integrations.GPT;
using AIWA.API.Models;
using Lli.OpenAi.Core.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AIWA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class VisionController(IStreamCache streamCache, IVisionService visionService, ISpeechService speechService) : ControllerBase
{
    [HttpPost("image")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostImageAsync(IFormFile userImage, string threadId, Guid guid)
    {
        var blockingCollection = streamCache.Cache.GetOrCreate(guid, f => new BlockingCollection<StreamQueueItem>());

        //var stream = AudioHub.GetAudioStream();
        var base64Image = GetBase64String(userImage);
        var response = await visionService.GetChatCompletionAsync(threadId, base64Image);
        var stream = await speechService.GetTtsStreamAsync(response, cancellationToken: HttpContext.RequestAborted)
            ?? throw new InvalidOperationException("Task is null");

        blockingCollection!.Add(new(stream));

        return AcceptedAtAction(actionName: nameof(SpeechController.GETStreamTtsResponseCached), controllerName: "Speech", routeValues: new { guid }, value: null);
    }

    /*
    [HttpPost("image")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostImageAsync(IFormFile userImage, string threadId)
    {
        if (userImage == null || userImage.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        //var guid = Guid.NewGuid();
        var guid = _guid;
        var base64Image = GetBase64String(userImage);

        var response = await visionService.GetChatCompletionAsync(threadId, base64Image);

        var stream = await speechService.GetTtsStreamAsync(response, cancellationToken: HttpContext.RequestAborted)
            ?? throw new InvalidOperationException("Task is null");

        var blockingCollection = streamCache.Cache.GetOrCreate(guid, f => new BlockingCollection<Stream>())!;
        blockingCollection.Add(stream);

        //_ = streamCache.Cache.Set(guid, task);
        //_ = streamCache.Cache.Set(guid, blockingCollection);

        return AcceptedAtAction(actionName: nameof(SpeechController.GETStreamTtsResponseAsyncCached), controllerName: "Speech", routeValues: new { guid }, value: null);

        //Console.WriteLine();
        //await foreach (var item in res)
        //{
        //    Console.Write(item);
        //}
        //Console.WriteLine();

        //return Results.Created();
    }
    */

    /*
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult POSTStreamTtsResponseCached(TtsRequest ttsRequest)
    {
        var guid = Guid.NewGuid();

        var task = speechservice.GetTtsStreamAsync(ttsRequest.Text, cancellationToken: HttpContext.RequestAborted)
            ?? throw new InvalidOperationException("Task is null");

        _ = streamCache.Cache.Set(guid, task);
        return AcceptedAtAction(nameof(GETStreamTtsResponseAsyncCached), value: null, routeValues: new { guid });
    }
    */

    private static string GetBase64String(IFormFile formFile)
    {
        using Stream fs = formFile.OpenReadStream();
        return Base64Writer.GetBase64String(fs);
    }
}
