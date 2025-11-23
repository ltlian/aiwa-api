namespace Lli.OpenAi.Core.Schema.Chat;

public record CreateChatCompletionResponse
(
    string Id,
    List<Choice> Choices,
    long Created,
    string Model,
    string? SystemFingerprint,
    string Object,
    CompletionUsage? Usage
);
