using Lli.OpenAi.Core.Speech;

namespace AIWA.API.Integrations.GPT;

public interface ISpeechService
{
    Task<Stream> GetTtsStreamAsync(string content, Voice voice = Voice.Nova, OutputFormat outputFormat = OutputFormat.Mp3, CancellationToken cancellationToken = default);
}
