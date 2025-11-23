namespace Lli.OpenAi.Core.Client;

public record PaginationParameters
(
    int? Limit = 20,
    string? Order = "desc",
    string? After = null,
    string? Before = null
);