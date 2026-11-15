using InventoryManager.API.Models.Common;
using InventoryManager.API.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TaskItem Configuration
        builder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(t => t.Priority).HasConversion<string>().HasMaxLength(50);

            entity.HasIndex(t => t.Title);
            entity.HasIndex(t => t.OwnerUserId);

            entity.HasOne(t => t.OwnerUser)
                  .WithMany(u => u.TaskItems)
                  .HasForeignKey(t => t.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Category Configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // InventoryItem Configuration
        builder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.SKU).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.ReorderLevel).HasDefaultValue(0);

            // Constraint: SKU is unique per owner
            entity.HasIndex(i => new { i.OwnerUserId, i.SKU }).IsUnique();

            // Search indexes
            entity.HasIndex(i => i.Name);
            entity.HasIndex(i => i.SKU);

            entity.HasOne(i => i.Category)
                  .WithMany(c => c.InventoryItems)
                  .HasForeignKey(i => i.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.OwnerUser)
                  .WithMany(u => u.InventoryItems)
                  .HasForeignKey(i => i.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StockMovement Configuration
        builder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(s => s.Quantity).IsRequired();

            entity.HasOne(s => s.InventoryItem)
                  .WithMany(i => i.StockMovements)
                  .HasForeignKey(s => s.InventoryItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken Configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(r => r.Token).IsUnique();

            entity.HasOne(r => r.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
