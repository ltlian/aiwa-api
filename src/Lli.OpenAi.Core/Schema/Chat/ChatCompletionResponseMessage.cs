namespace Lli.OpenAi.Core.Schema.Chat;

public record ChatCompletionResponseMessage(string Content)
{
    public string Role { get; } = "assistant";
    public override string ToString() => $"{Role}: {Content}";
};
