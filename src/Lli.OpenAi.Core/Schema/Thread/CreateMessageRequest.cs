using Lli.OpenAi.Core.Client;

namespace Lli.OpenAi.Core.Schema.Thread;

public record CreateMessageRequest
(
    string Role,
    string Content,
    IEnumerable<string>? FileIds = null,
    Dictionary<string, string>? Metadata = null
) : IOpenAiRequest;
