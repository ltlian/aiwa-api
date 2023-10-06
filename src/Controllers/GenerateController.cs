using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using AIWA.API.Models;
using AIWA.API.Integrations.GPT4;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AIWA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GenerateController(ILogger<GenerateController> logger, UkesMailCompletion ukesMailCompletion) : ControllerBase
{
    private readonly ILogger<GenerateController> _logger = logger;
    private readonly UkesMailCompletion _ukesMailCompletion = ukesMailCompletion;
    private const int MAX_INPUT_LINES = 5;
    private const int MAX_INPUT_CHARS = 1000;

    /// <summary>
    /// Generates a new ukesmail.
    /// </summary>
    /// <param name="input">Custom input parameters.</param>
    /// <param name="accept">Response type.</param>
    /// <remarks>
    /// Use the Accept header to specify the response type.
    /// - "application/json" (default): receive the content as a HTTP response.
    /// - "text/event-stream": receive the content as a chunked stream.
    /// </remarks>
    [HttpPost("ukesmail")]
    [Produces(MediaTypeNames.Application.Json, "text/event-stream")]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostAsync([FromHeader(Name = "accept")] string? accept, AIWAInput input, CancellationToken cancellationToken)
    {
        var validationResult = ValidateInput(input);
        if (!validationResult.IsSuccess)
        {
            return new ObjectResult(validationResult.Error) { StatusCode = (int)validationResult.Value };
        }

        switch (accept)
        {
            case MediaTypeNames.Application.Json:
            case "":
            case "*/*":
            case null:
                var messages = await _ukesMailCompletion.GenerateUkesMailAsync(input.Entries, choices: 1, cancellationToken);
                return new JsonResult(new AIWAResponse { Content = messages.First().Content });
            case "text/event-stream":
                var streamingResult = await _ukesMailCompletion.GetUkesMailStreamingAsync(input.Entries, cancellationToken);
                return new ChunkedStreamingChatChoiceResult(streamingResult);
            default:
                var problemDetails = new ProblemDetails
                {
                    Detail = $"'Accept' header specifies an Invalid format. Expected formats are '${MediaTypeNames.Application.Json}' (default) or 'text/event-stream'.",
                    Status = 400
                };

                return BadRequest(problemDetails);
        }
    }

    private static Result<HttpStatusCode> ValidateInput(AIWAInput input)
    {
        if (input.Entries.Sum(x => x.Length) > MAX_INPUT_CHARS)
        {
            return new Result<HttpStatusCode>(false, HttpStatusCode.RequestEntityTooLarge, $"A total of {MAX_INPUT_CHARS} input characters are allowed at most.");
        }

        if (input.Entries.Count() > MAX_INPUT_LINES)
        {
            return new Result<HttpStatusCode>(false, HttpStatusCode.RequestEntityTooLarge, $"A total of {MAX_INPUT_LINES} input entries are allowed at most.");
        }

        return Result<HttpStatusCode>.Success(HttpStatusCode.OK);
    }
}
