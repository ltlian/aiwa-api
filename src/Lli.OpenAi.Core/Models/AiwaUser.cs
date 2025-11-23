namespace Lli.OpenAi.Core.Models;

public class AiwaUser : IAiwaEntity
{
    public required Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
}
