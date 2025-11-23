using Lli.OpenAi.Core.Client;

namespace Lli.OpenAi.Core.Speech;

public record CreateSpeechRequest
(
    TtsModel Model,
    string Input,
    Voice Voice,
    OutputFormat? ResponseFormat = null,
    float? Speed = null
) : IOpenAiRequest;
