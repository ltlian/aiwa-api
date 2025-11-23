namespace Lli.OpenAi.Core.Schema.Chat;

public record CompletionUsage(int PromptTokens, int CompletionTokens, int TotalTokens);
