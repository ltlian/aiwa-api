using System.Runtime.CompilerServices;

using AIWA.EntityFrameworkCore;

using Lli.OpenAi.Core.Models;

using Microsoft.EntityFrameworkCore;

namespace AIWA.API.Data;

public class AiwaEFDataStore(IDbContextFactory<AiwaDbContext> dbContextFactory) : IDataStore
{
    public async Task<AiwaUser> CreateUserAsync(string username, CancellationToken cancellationToken)
    {
        var user = new AiwaUser
        {
            Name = username,
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Users.Add(user);
        _ = await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<Guid> AddUserAsync(AiwaUser user, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Users.Add(user);
        _ = await dbContext.SaveChangesAsync(cancellationToken);
        return user.Id;
    }

    public async Task<AiwaUser?> GetUserOrDefaultAsync(Guid id, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken: cancellationToken);
    }

    public async IAsyncEnumerable<InteractionUnit> GetInteractionUnitsAsync(Guid messageId, int? limitEntries = null, TimeSpan? limitTime = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int count = 0;
        var visitedIds = new HashSet<Guid>();
        var deadline = limitTime.HasValue ? DateTimeOffset.UtcNow.Add(limitTime.Value) : DateTimeOffset.MaxValue;

        using var dbContext = dbContextFactory.CreateDbContext();

        // Get the initial message
        while (await GetFirstOrDefaultAsync(dbContext, messageId, cancellationToken) is InteractionUnit iu)
        {
            // The initial fetch using 'GetFirstOrDefaultAsync' always loads parents, and a parent can never be null as
            // a root message references itself.
            var parentEntryIsLoaded = true;
            var candidate = iu;

            while (parentEntryIsLoaded)
            {
                yield return candidate;

                if (!visitedIds.Add(candidate.Id) || candidate.IsRoot())
                    yield break;

                if (count++ >= limitEntries || DateTimeOffset.UtcNow >= deadline)
                    yield break;

                parentEntryIsLoaded = dbContext.Entry(candidate).Reference(iu => iu.Parent).IsLoaded && candidate.Parent is not null;
                if (parentEntryIsLoaded)
                    candidate = candidate.Parent!;
            }

            // At this point, the parent exists, is not the entity itself, but is not loaded.
            // We can use the current candidate's parentId to fetch the next batch.
            messageId = candidate.ParentId;
        }
    }

    private static async Task<InteractionUnit?> GetFirstOrDefaultAsync(AiwaDbContext dbContext, Guid messageId, CancellationToken cancellationToken) =>
        await dbContext.InteractionUnits
            // Define some number of include levels so that they are known to EF at compile time.
            // Adjust include levels as needed.
            .Include(iu => iu.Parent)
            .ThenInclude(iup => iup.Parent)
            .FirstOrDefaultAsync(iu => iu.Id == messageId, cancellationToken);

    public async Task AddInteractionUnitsAsync(IEnumerable<InteractionUnit> interactionUnits, CancellationToken cancellationToken = default)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.InteractionUnits.AddRange(interactionUnits);
        _ = await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<InteractionUnit?> GetLastUserInteractionUnitOrDefaultAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var ius = await dbContext.InteractionUnits
            .Where(iu => iu.UserId == userId)
            .Where(iu => iu.Id != iu.ParentId)
            .ToListAsync(cancellationToken);

        return ius.OrderByDescending(iu => iu.CreatedAt).FirstOrDefault();
    }
}
