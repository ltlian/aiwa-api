namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionChoice
(
    int Index,
    ChatCompletionMessage Message,
    string FinishReason
);
