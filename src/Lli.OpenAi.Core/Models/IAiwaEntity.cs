namespace Lli.OpenAi.Core.Models;

public interface IAiwaEntity
{
    public Guid Id { get; set; }
    DateTimeOffset CreatedAt { get; }
}
