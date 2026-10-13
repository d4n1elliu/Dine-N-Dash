using DineNDash.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DineNDash.Data;
/// <summary>
/// EF Core context for the DineNDash database.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<DinerAccount> Diners => Set<DinerAccount>();
    public DbSet<RestaurantAccount> RestaurantOwners => Set<RestaurantAccount>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("AccountType")
            .HasValue<DinerAccount>("Diner")
            .HasValue<RestaurantAccount>("RestaurantOwner");

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Ignore(u => u.Role);

        modelBuilder.Entity<Restaurant>()
            .HasOne(r => r.Owner)
            .WithOne(o => o.Restaurant)
            .HasForeignKey<Restaurant>(r => r.OwnerId);

        modelBuilder.Entity<MenuItem>().Property(m => m.Price).HasPrecision(10, 2);

        modelBuilder.Entity<Review>().HasIndex(r => new { r.DinerId, r.RestaurantId }).IsUnique();
    }

    /// <summary>
    /// Sets UpdatedAt automatically whenever a model is changed.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Modified))
            entry.Entity.UpdatedAt = DateTime.UtcNow;

        return base.SaveChangesAsync(cancellationToken);
    }
}