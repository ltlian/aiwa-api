namespace Lli.OpenAi.Core.Schema.Chat;

public enum FinishReason
{
    Stop = 1,
    Length = 2,
    ToolCalls = 3,
    ContentFilter = 4,
    FunctionCall = 5
}
