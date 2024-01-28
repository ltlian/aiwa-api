using AIWA.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AIWA.API.Data.EF;

public class AiwaSQLiteContext(DbContextOptions<AiwaSQLiteContext> options) : DbContext(options)
{
    public DbSet<AiwaUser> Users { get; set; }
    public DbSet<InteractionUnit> InteractionUnits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure self-referencing relationship
        modelBuilder.Entity<InteractionUnit>()
            .HasOne(i => i.Parent)
            .WithMany()
            .HasForeignKey(i => i.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InteractionUnit>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId);
    }
}
