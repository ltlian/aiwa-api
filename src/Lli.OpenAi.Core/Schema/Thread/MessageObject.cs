namespace Lli.OpenAi.Core.Schema.Thread;

public record MessageObject
(
    string Id,
    string Object,
    long CreatedAt,
    string ThreadId,
    string Role,
    IMessageContentObject[] Content,
    string? AssistantId,
    string? RunId,
    string[]? FileIds,
    IDictionary<string, object>? Metadata
);
