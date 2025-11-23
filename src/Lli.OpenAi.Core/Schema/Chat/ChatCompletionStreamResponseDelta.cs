namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionStreamResponseDelta
(
    string Role,
    string? Content
);