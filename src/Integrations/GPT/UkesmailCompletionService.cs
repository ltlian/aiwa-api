using System.Text;
using AIWA.API.Globalization;
using Lli.OpenAi.Core.Client;
using Lli.OpenAi.Core.Schema.Chat;
using Microsoft.Extensions.Options;
using static Lli.OpenAi.Core.Schema.Models;

namespace AIWA.API.Integrations.GPT;

public class UkesmailCompletionService(IOptions<OpenAIOptions> options, ILogger<UkesmailCompletionService> logger, IOpenAiHttpClient openAiHttpClient) : IUkesmailCompletion
{
    private const string SYSTEM_PROMPT =
    """
    Du er en virtuell agent som bistår med å formulere e-postmeldinger, og svarer i form av en e-post som ønskes formulert. Mine meldinger til deg er todelt hvor første del er statisk definert, og andre halvdel er skrevet inn av brukeren som skal sende e-posten du skriver.
    """;

    private const string USER_PROMPT =
    """
    Du er enhetsleder for enheten ACME. En enhet er en form for avdeling i en bedrift.

    ACME er del av Bouvet som er et større selskap. Enheten har kontor sørvest i Norge. De ansatte i ACME består av rundt 30 IT-utviklere.

    I slutten av hver uke skriver du en email som går ut til disse ansatte, kalt ukesmail. De ansatte setter pris på disse mailene og synes de er koselige.

    Disse mailene er korte og uformelle med en avslappet tone.

    Noen ganger er det oppdateringer rundt hva som foregår med prosjekter i enheten.

    Noen ganger og hvis det ikke er mye annet å snakke om, nevnes været i forbindelse med årstiden.

    Hvis det er lite å snakke om, er det vanlig at mailen er i form av en liten hilsen.

    Dags dato er {0}.

    Det er vedlagt ulik informasjon du kan bruke til denne ukens ukesmail. Denne informasjonen er fritekst og skrevet inn av brukeren. Denne informasjonen kommer under.
    """;

    public IAsyncEnumerable<CreateChatCompletionStreamResponse> GetUkesMailStreamingAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default)
    {
        var extrasCount = promptExtras.Count();
        var chatCompletionsOptions = GetChatCompletionsOptions(promptExtras, options.Value.Model ?? GPT35_turbo, 1, stream: true);
        logger.LogInformation("Requesting {DeploymentId} for {Choices} choices with {ExtrasCount} extra parameters", chatCompletionsOptions.Model, 1, extrasCount);
        return openAiHttpClient.CreateChatCompletionStreamedAsync(chatCompletionsOptions, cancellationToken);
    }

    public async Task<CreateChatCompletionResponse> GenerateUkesMailAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default)
    {
        var chatCompletionsOptions = GetChatCompletionsOptions(promptExtras, options.Value.Model ?? GPT35_turbo, 1);
        logger.LogInformation("Requesting {DeploymentId} for {Choices} choices with {ExtrasCount} extra parameters", chatCompletionsOptions.Model, 1, promptExtras.Count());

        var result = await openAiHttpClient.CreateChatCompletionAsync(chatCompletionsOptions, cancellationToken);
        return result ?? throw new NullReferenceException(nameof(result));
    }

    private static CreateChatCompletionRequest GetChatCompletionsOptions(IEnumerable<string> promptExtras, string model, int choiceCount = 1, bool stream = false)
    {
        var dateNowString = DateOnly.FromDateTime(DateTime.Now).ToString("d MMMM", NorwegianCulture.DateTimeFormatInfo);
        var promptBuilder = new StringBuilder(string.Format(USER_PROMPT, dateNowString) + Environment.NewLine);

        foreach (var promptExtra in promptExtras)
        {
            promptBuilder.AppendLine(promptExtra);
        }

        List<IChatCompletionRequestMessage> messages = [
            new ChatCompletionRequestSystemMessage(SYSTEM_PROMPT),
            new ChatCompletionRequestUserMessage([new ChatCompletionRequestMessageContentPartText(promptBuilder.ToString())])
        ];

        return new CreateChatCompletionRequest(messages, model, MaxTokens: 1000, N: choiceCount, Stream: stream);
    }
}
