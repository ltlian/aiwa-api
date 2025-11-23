namespace Lli.OpenAi.Core.Schema.Chat;

public record ChoiceDelta
(
    ChatCompletionStreamResponseDelta Delta,
    FinishReason? FinishReason,
    int Index
);
