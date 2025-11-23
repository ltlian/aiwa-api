namespace Lli.OpenAi.Core.Schema.Chat;

public record Choice
(
    ChatCompletionResponseMessage Message,
    FinishReason FinishReason,
    int Index
);
