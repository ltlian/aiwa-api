using System.Collections.Concurrent;
using AIWA.API.Integrations.GPT;
using AIWA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AIWA.API.Controllers;

[Route("[controller]")]
[ApiController]
public class SpeechController(IStreamCache streamCache) : ControllerBase
{
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
