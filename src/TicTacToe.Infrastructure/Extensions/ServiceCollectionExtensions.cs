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
        // add DB context - context for PostgresSQL
        services.AddDbContext<TicTacToeDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        // add repos
        services.AddScoped<IGameRepository, GameRepository>();

        // add health checks for DB
        services.AddHealthChecks()
            .AddDbContextCheck<TicTacToeDbContext>();

        return services;
    }
}