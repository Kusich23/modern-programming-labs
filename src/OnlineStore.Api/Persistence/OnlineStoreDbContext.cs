using Microsoft.EntityFrameworkCore;

namespace OnlineStore.Api.Persistence;

/// <summary>
/// Entity Framework Core database context for persisting online store orders.
/// </summary>
public sealed class OnlineStoreDbContext(DbContextOptions<OnlineStoreDbContext> options) : DbContext(options)
{
    public DbSet<OrderRow> Orders => Set<OrderRow>();
    public DbSet<OrderLineRow> OrderLines => Set<OrderLineRow>();
    public DbSet<ReturnRequestRow> ReturnRequests => Set<ReturnRequestRow>();
    public DbSet<ReturnItemRow> ReturnItems => Set<ReturnItemRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderRow>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Status).IsRequired().HasMaxLength(32);
            entity.HasMany(o => o.Lines)
                .WithOne(l => l.OrderRow)
                .HasForeignKey(l => l.OrderRowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLineRow>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ProductName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<ReturnRequestRow>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Reason).IsRequired().HasMaxLength(500);
            entity.Property(r => r.Status).IsRequired().HasMaxLength(32);
            entity.HasMany(r => r.Items)
                .WithOne(i => i.ReturnRequestRow)
                .HasForeignKey(i => i.ReturnRequestRowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReturnItemRow>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        });
    }
}
