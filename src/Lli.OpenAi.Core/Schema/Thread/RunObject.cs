namespace Lli.OpenAi.Core.Schema.Thread;

public record RunObject
(
    string Id,
    string Object,
    long CreatedAt,
    string AssistantId,
    string ThreadId,
    string Status,
    long? StartedAt,
    long? ExpiresAt,
    long? CancelledAt,
    long? FailedAt,
    long? CompletedAt,
    Dictionary<string, object>? LastError, // To be implemented later
    string Model, // To be implemented later
    string Instructions,
    IAssistantTool[]? Tools,
    string[]? FileIds,
    Dictionary<string, object>? Metadata // To be implemented later
);
