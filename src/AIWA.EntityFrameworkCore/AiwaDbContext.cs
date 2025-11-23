using AIWA.EntityFrameworkCore.EntityTypeConfiguration;

using Lli.OpenAi.Core.Models;

using Microsoft.EntityFrameworkCore;

namespace AIWA.EntityFrameworkCore;

public class AiwaDbContext(DbContextOptions<AiwaDbContext> options) : DbContext(options)
{
    public DbSet<AiwaUser> Users { get; set; }
    public DbSet<InteractionUnit> InteractionUnits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new InteractionUnitTypeConfiguration());
    }
}
