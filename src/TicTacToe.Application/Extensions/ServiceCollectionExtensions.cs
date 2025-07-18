using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Application.UseCases;

namespace TicTacToe.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // register use cases
        services.AddScoped<CreateGameUseCase>();
        services.AddScoped<GetGameUseCase>();
        services.AddScoped<MakeMoveUseCase>();

        return services;
    }
}
