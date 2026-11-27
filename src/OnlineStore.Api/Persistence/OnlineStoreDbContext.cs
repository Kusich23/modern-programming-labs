using Microsoft.EntityFrameworkCore;

namespace OnlineStore.Api.Persistence;

/// <summary>
/// Entity Framework Core database context for persisting online store orders.
/// </summary>
public sealed class OnlineStoreDbContext(DbContextOptions<OnlineStoreDbContext> options) : DbContext(options)
{
    public DbSet<OrderRow> Orders => Set<OrderRow>();
    public DbSet<OrderLineRow> OrderLines => Set<OrderLineRow>();

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
    }
}
