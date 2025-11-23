using Lli.OpenAi.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIWA.EntityFrameworkCore.EntityTypeConfiguration;

public class InteractionUnitTypeConfiguration : IEntityTypeConfiguration<InteractionUnit>
{
    public void Configure(EntityTypeBuilder<InteractionUnit> builder)
    {
        // Configure self-referencing relationship
        builder.HasOne(i => i.Parent)
            .WithMany()
            .HasForeignKey(i => i.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId);
    }
}
