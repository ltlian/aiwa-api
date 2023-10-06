using System.Net;
using System.Net.Mime;
using AIWA.API.Integrations.GPT4;
using AIWA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AIWA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GenerateController(ILogger<GenerateController> logger, UkesMailCompletion ukesMailCompletion) : ControllerBase
{
    private readonly ILogger<GenerateController> _logger = logger;
    private readonly UkesMailCompletion _ukesMailCompletion = ukesMailCompletion;
    private const string TextEventStream = "text/event-stream";
    private static readonly List<MediaTypeHeaderValue> _acceptStreamHeaders = [new(TextEventStream)];
    private static readonly List<MediaTypeHeaderValue> _acceptJsonHeaders = [new(MediaTypeNames.Application.Json), new("*/*")];
    private const int MAX_INPUT_LINES = 5;
    private const int MAX_INPUT_CHARS = 1000;

    /// <summary>
    /// Generates a new ukesmail.
    /// </summary>
    /// <param name="input">Custom input parameters.</param>
    /// <param name="accept">Response type.</param>
    /// <remarks>
    /// The Accept header is optional. By default, the method will return application/json.
    /// - "application/json" (default): receive the content as JSON in a regular HTTP response.
    /// - "text/event-stream": receive the content as plain text in a chunked stream.
    /// </remarks>
    [HttpPost("ukesmail")]
    [Produces(MediaTypeNames.Application.Json, TextEventStream)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostAsync(AIWAInput input, CancellationToken cancellationToken)
    {

        var validationResult = ValidateInput(input);
        if (!validationResult.IsSuccess)
        {
            return Problem
            (
               title: "Invalid input",
               detail: validationResult.Error,
               statusCode: (int)validationResult.Value
            );
        }

        var requestAcceptHeader = HttpContext.Request.GetTypedHeaders().Accept;

        if (requestAcceptHeader.Count == 0 || requestAcceptHeader.Any(_acceptStreamHeaders.Contains))
        {
            var streamingResult = await _ukesMailCompletion.GetUkesMailStreamingAsync(input.Entries, cancellationToken);
            return new ChunkedStreamingChatChoiceResult(streamingResult);
        }
        else if (requestAcceptHeader.Any(_acceptJsonHeaders.Contains))
        {
            var messages = await _ukesMailCompletion.GenerateUkesMailAsync(input.Entries, choices: 1, cancellationToken);
            return new JsonResult(new AIWAResponse { Content = messages.First().Content });
        }
        else
        {
            return Problem
            (
                title: "Invalid format",
                detail: $"The format specified by the 'Accept' header is not valid. Expected formats are '{MediaTypeNames.Application.Json}' (default) or '{TextEventStream}'.",
                statusCode: StatusCodes.Status400BadRequest
            );
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
