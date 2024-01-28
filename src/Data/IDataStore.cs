using System.Runtime.CompilerServices;
using AIWA.API.Data.Models;

namespace AIWA.API.Data;

public interface IDataStore
{
    Task<AiwaUser> CreateUserAsync(string username, CancellationToken cancellationToken);
    Task AddInteractionUnitsAsync(IEnumerable<InteractionUnit> interactionUnits, CancellationToken cancellationToken = default);
    Task<InteractionUnit?> GetLastUserInteractionUnitOrDefaultAsync(Guid userId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<InteractionUnit> GetInteractionUnitsAsync(Guid messageId, int? limitEntries = null, TimeSpan? limitTime = null, CancellationToken cancellationToken = default);
    Task<AiwaUser?> GetUserOrDefaultAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> AddUserAsync(AiwaUser user, CancellationToken cancellationToken);
}
