using System.Collections.Concurrent;
using AIWA.API.Integrations.GPT;
using AIWA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AIWA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeechController(ISpeechService speechservice, IStreamCache streamCache) : ControllerBase
{
    /* Not in use
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> POSTStreamTtsResponseCached(TtsRequest ttsRequest)
    {
        var guid = Guid.NewGuid();

        var stream = await speechservice.GetTtsStreamAsync(ttsRequest.Text, cancellationToken: HttpContext.RequestAborted);
        // var stream = AudioHub.GetAudioStream();

        var blockingCollection = new BlockingCollection<StreamQueueItem>
        {
            new(stream)
        };

        return AcceptedAtAction(nameof(GETStreamTtsResponseCached), value: null, routeValues: new { guid });
    }
    */

    [HttpGet("{guid}")]
    public IResult GETStreamTtsResponseCached(Guid guid)
    {
        var blockingCollection = streamCache.Cache.Get<BlockingCollection<StreamQueueItem>>(guid)!;

        foreach (var item in blockingCollection.GetConsumingEnumerable())
        {
            if (!item.Stream.CanRead)
            {
                continue;
            }

            return Results.Stream(item.Stream);
        }

        throw new InvalidOperationException();
    }
}
