using AIWA.API.Data.Models;
using AIWA.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIWA.API.Data.EF;

//public class AiwaNpgDbContext : DbContext
//{
//    public DbSet<InteractionUnit> ThreadMessages { get; set; }
//    public DbSet<InteractionFlow> ImageConversationThreads { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<InteractionUnit>(entity =>
//        {
//            entity.HasKey(e => e.Id).HasName("message_pkey");

//            entity.ToTable("thread_message");

//            entity.Property(e => e.Content).HasColumnName("content");
//            entity.Property(e => e.Role).HasColumnName("role");
//            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
//        });

//        modelBuilder.Entity<InteractionFlow>(entity =>
//        {
//            entity.HasKey(e => e.Id).HasName("thread_pkey");

//            entity.ToTable("thread");

//            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
//        });

//        base.OnModelCreating(modelBuilder);
//    }
//}
