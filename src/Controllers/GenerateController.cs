using System.Net;
using System.Net.Mime;
using AIWA.API.Integrations.GPT;
using AIWA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace AIWA.API.Controllers;

[Route("[controller]")]
[ApiController]
public class GenerateController(IChatCompletion chatCompletionService) : ControllerBase
{
    private const int MAX_INPUT_LINES = 5;
    private const int MAX_INPUT_CHARS = 1000;
    private const string TextEventStream = "text/event-stream";
    private static readonly List<MediaTypeHeaderValue> _acceptStreamHeaders = [new(TextEventStream)];
    private static readonly List<MediaTypeHeaderValue> _acceptJsonHeaders = [new(MediaTypeNames.Application.Json), new("*/*")];

    /// <summary>
    /// Generates a new chat completion.
    /// </summary>
    /// <param name="input">Custom input parameters.</param>
    /// <param name="accept">Response type.</param>
    /// <remarks>
    /// The Accept header is optional. By default, the method will return application/json.
    /// - "application/json" (default): receive the content as JSON in a regular HTTP response.
    /// - "text/event-stream": receive the content as plain text in a chunked stream.
    /// </remarks>
    [HttpPost("chat")]
    [Produces(MediaTypeNames.Application.Json, TextEventStream)]
    [ProducesResponseType<AIWAResponse>(StatusCodes.Status200OK, Type = typeof(AIWAResponse))]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> PostAsync(ChatCompletionInput input, CancellationToken cancellationToken)
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

        if (requestAcceptHeader.Count == 0 || requestAcceptHeader.Any(_acceptJsonHeaders.Contains))
        {
            var response = await chatCompletionService.CreateChatCompletionAsync(input.Content, cancellationToken);
            return new JsonResult(new AIWAResponse { Content = response.Choices[0].Message.Content });
        }
        else if (requestAcceptHeader.Any(_acceptStreamHeaders.Contains))
        {
            var streamingResult = chatCompletionService.CreateChatCompletionStreamedAsync(input.Content, cancellationToken);
            return new StreamingChatTextResult(streamingResult);
        }
        else
        {
            return Problem
            (
                title: "Invalid format",
                detail: $"The format specified by the 'Accept' header is invalid. Expected formats are '{MediaTypeNames.Application.Json}' (default) or '{TextEventStream}'.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }

    private static Result<HttpStatusCode> ValidateInput(ChatCompletionInput input) => input.Content.Length > MAX_INPUT_CHARS
            ? new Result<HttpStatusCode>(false, HttpStatusCode.RequestEntityTooLarge, $"A total of {MAX_INPUT_CHARS} input characters are allowed at most.")
            : Result<HttpStatusCode>.Success(HttpStatusCode.OK);
}
