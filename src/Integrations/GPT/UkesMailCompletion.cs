using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using Azure.AI.OpenAI;
using AIWA.API.Globalization;

namespace AIWA.API.Integrations.GPT4;

public class UkesMailCompletion(IOptions<OpenAIOptions> options, ILogger<UkesMailCompletion> logger)
{
    private readonly string _deploymentId = options.Value.Model;
    private readonly ILogger<UkesMailCompletion> _logger = logger;
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

    private readonly OpenAIClient _openAIClient = new(options.Value.OpenAIKey);

    public async Task<IAsyncEnumerable<StreamingChatChoice>> GetUkesMailStreamingAsync(IEnumerable<string> promptExtras, CancellationToken cancellationToken = default)
    {
        var extrasCount = promptExtras.Count();
        _logger.LogInformation($"Requesting {_deploymentId} for {1} choices with {extrasCount} extra parameters");
        var chatCompletionsOptions = GetChatCompletionsOptions(promptExtras, 1);

        try
        {
            var response = await _openAIClient.GetChatCompletionsStreamingAsync(_deploymentId, chatCompletionsOptions, cancellationToken);
            return response.Value.GetChoicesStreaming(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenAI");
            throw;
        }
    }

    public async Task<List<ChatMessage>> GenerateUkesMailAsync(IEnumerable<string> promptExtras, int? choices = null, CancellationToken cancellationToken = default)
    {
        var extrasCount = promptExtras.Count();
        var choicesCount = choices ?? 1;
        var sw = Stopwatch.StartNew();
        _logger.LogInformation($"Requesting {_deploymentId} for {choicesCount} choices with {extrasCount} extra parameters");
        var completionOptions = GetChatCompletionsOptions(promptExtras, choicesCount);
        var response = await _openAIClient.GetChatCompletionsAsync(_deploymentId, completionOptions);
        _logger.LogInformation($"Got response after {sw.ElapsedMilliseconds} ms");
        return response.Value.Choices.Select(c => c.Message).ToList();
    }

    private static ChatCompletionsOptions GetChatCompletionsOptions(IEnumerable<string> promptExtras, int choiceCount = 1)
    {
        var dateNowString = DateOnly.FromDateTime(DateTime.Now).ToString("d MMMM", NorwegianCulture.DateTimeFormatInfo);
        var promptBuilder = new StringBuilder(string.Format(USER_PROMPT, dateNowString) + Environment.NewLine);

        foreach (var promptExtra in promptExtras)
        {
            promptBuilder.AppendLine(promptExtra);
        }

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SYSTEM_PROMPT),
            new(ChatRole.User, promptBuilder.ToString())
        };

        return new ChatCompletionsOptions(messages)
        {
            MaxTokens = 5000,
            Temperature = 1.0f,
            FrequencyPenalty = 0.0f,
            PresencePenalty = 0.0f,
            ChoiceCount = choiceCount
        };
    }
}
