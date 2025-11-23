using System.Collections.Concurrent;
using System.Net;
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
    public async Task<IActionResult> PostImageAsync(IFormFile userImage, Guid threadId, Guid guid, CancellationToken cancellationToken)
    {
        var blockingCollection = streamCache.Cache.GetOrCreate(guid, f => new BlockingCollection<StreamQueueItem>());

        var base64Image = GetBase64String(userImage);
        var response = await visionService.GetChatCompletionAsync(threadId, base64Image, cancellationToken);
        var stream = await speechService.GetTtsStreamAsync(response, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Task is null");

        blockingCollection!.Add(new(stream), cancellationToken);

        return AcceptedAtAction(actionName: nameof(SpeechController.GETStreamTtsResponseCached), controllerName: "Speech", routeValues: new { guid }, value: null);
    }

    [HttpPost("image/enhance")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostEnhanceImageAsync(IFormFile userImage, CancellationToken cancellationToken)
    {
        var validationResult = ValidateInput(userImage);
        if (!validationResult.IsSuccess)
        {
            return Problem(validationResult.Error, statusCode: (int)validationResult.Value);
        }

        var base64Image = GetBase64String(userImage);
        var description = await visionService.DescribeImageAsync(base64Image, cancellationToken);
        var uri = await visionService.GenerateImageAsync(description, cancellationToken);
        return Created(uri, null);
    }

    private const int MaxFileSize = 64 * 1024;
    private static Result<HttpStatusCode> ValidateInput(IFormFile formFile) => formFile.Length > MaxFileSize
        ? new Result<HttpStatusCode>(false, HttpStatusCode.RequestEntityTooLarge, $"File size cannot exceed {MaxFileSize} bytes.")
        : Result<HttpStatusCode>.Success(HttpStatusCode.OK);

    private static string GetBase64String(IFormFile formFile)
    {
        using Stream fs = formFile.OpenReadStream();
        var base64 = Base64Writer.GetBase64String(fs);
        return base64.StartsWith("data:image/png;base64,")
            ? base64
            : $"data:image/png;base64,{base64}";
    }
}
