using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Domain.Interfaces;
using TicTacToe.Infrastructure.Persistence;
using TicTacToe.Infrastructure.Repositories;

namespace TicTacToe.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // add DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("localhost"))
        {
            // use InMemory DB for development/testing
            // - when PostgreSQL is not available
            services.AddDbContext<TicTacToeDbContext>(options =>
            {
                options.UseInMemoryDatabase("TicTacToeInMemory");
            });
        }
        else
        {
            // use PostgreSQL for production
            services.AddDbContext<TicTacToeDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
        }

        // add repos
        services.AddScoped<IGameRepository, GameRepository>();

        // add health checks for DB
        services.AddHealthChecks()
            .AddDbContextCheck<TicTacToeDbContext>();

        return services;
    }
}