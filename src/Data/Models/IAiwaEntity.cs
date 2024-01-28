namespace AIWA.API.Data.Models;

public interface IAiwaEntity
{
    public Guid Id { get; set; }
    DateTimeOffset CreatedAt { get; }
}
