using Lli.OpenAi.Core.Client;

namespace Lli.OpenAi.Core.Schema.Thread;

public record CreateThreadRequest
(
    IEnumerable<CreateMessageRequest>? Messages = null,
    Dictionary<string, string>? Metadata = null
) : IOpenAiRequest;
