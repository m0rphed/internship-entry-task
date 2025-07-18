using Microsoft.EntityFrameworkCore;
using TicTacToe.Domain.Entities;
using TicTacToe.Infrastructure.Persistence.Configurations;
using System.Text.Json;

namespace TicTacToe.Infrastructure.Persistence;

public class TicTacToeDbContext : DbContext
{
    public TicTacToeDbContext(DbContextOptions<TicTacToeDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // apply all configurations
        modelBuilder.ApplyConfiguration(new GameConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // all DateTime properties should use UTC
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<DateTimeUtcConverter>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // update timestamps before saving
        var entries = ChangeTracker.Entries<Game>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // set UpdatedAt using reflection, bc. it's private
            // TODO: find better solution for setting `UpdateAt`
            var updateProperty = entry.Property("UpdatedAt");
            updateProperty.CurrentValue = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

// converter ensures DateTime values are stored as UTC
public class
    DateTimeUtcConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter() : base(
        dateTime => dateTime.ToUniversalTime(),
        dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
    {
    }
}