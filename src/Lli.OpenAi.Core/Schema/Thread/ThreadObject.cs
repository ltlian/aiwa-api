namespace Lli.OpenAi.Core.Schema.Thread;

public record ThreadObject
(
    string Id,
    string Object,
    long CreatedAt,
    Dictionary<string, string>? Metadata = null
);
