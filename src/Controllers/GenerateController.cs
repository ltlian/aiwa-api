using System.Net;
using System.Net.Mime;
using AIWA.API.Integrations.GPT;
using AIWA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AIWA.API.Controllers;

[ApiController]
[Route("[controller]")]
public class GenerateController(ILogger<GenerateController> logger, IChatCompletion chatCompletionService) : ControllerBase
{
    private readonly ILogger<GenerateController> _logger = logger;
    private readonly IChatCompletion _chatCompletionService = chatCompletionService;
    private const string TextEventStream = "text/event-stream";
    private static readonly List<MediaTypeHeaderValue> _acceptStreamHeaders = [new(TextEventStream)];
    private static readonly List<MediaTypeHeaderValue> _acceptJsonHeaders = [new(MediaTypeNames.Application.Json), new("*/*")];
    private const int MAX_INPUT_LINES = 5;
    private const int MAX_INPUT_CHARS = 1000;

    /**/
    [HttpPost("chat")]
    [Produces(MediaTypeNames.Application.Json, TextEventStream)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostAsync(AIWAInput input, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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

        // First, check for JSON, or if no Accept header is provided, default to JSON.
        if (requestAcceptHeader.Count == 0 || requestAcceptHeader.Any(_acceptJsonHeaders.Contains))
        {
            //var r = await _chatCompletionService.OpenAiHttpClient.CreateChatCompletionAsync();
            //return new JsonResult(new AIWAResponse { Content = messages.First().Content });
        }
        else if (requestAcceptHeader.Any(_acceptStreamHeaders.Contains))
        {
            //var streamingResult = await _chatCompletionService.GetChatCompletionStreamAsync(input.Entries.First(), cancellationToken);
            //return new ChunkedStreamingChatCompletionResult(streamingResult);

            // using (var stream = await response.Content.ReadAsStreamAsync())
            // using (var reader = new StreamReader(stream))
            // {
            //     while (!reader.EndOfStream)
            //     {
            //         var chunk = await reader.ReadLineAsync();
            //         // Process each chunk
            //         Console.WriteLine(chunk);
            //     }
            // }
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
    /**/
}
