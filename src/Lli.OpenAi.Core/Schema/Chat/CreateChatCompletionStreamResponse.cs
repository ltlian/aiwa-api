namespace Lli.OpenAi.Core.Schema.Chat;

public record CreateChatCompletionStreamResponse
(
    string Id,
    List<ChoiceDelta> Choices,
    long Created,
    string Model,
    string? SystemFingerprint,
    string Object
);
