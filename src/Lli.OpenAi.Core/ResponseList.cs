namespace Lli.OpenAi.Core;

public record ResponseList<T>
(
    List<T> Data,
    string Object,
    bool HasMore,
    string? FirstId = null,
    string? LastId = null
);
