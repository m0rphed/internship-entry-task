using TicTacToe.Application.DTOs;
using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Interfaces;
using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Application.UseCases;

public class CreateGameUseCase
{
    private readonly IGameRepository _gameRepository;

    public CreateGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<CreateGameResponse> ExecuteAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        var gameId = Guid.NewGuid();
        
        var game = new Game(
            gameId,
            request.BoardSize,
            request.WinCondition,
            request.FirstPlayer,
            request.RandomMoveChance,
            request.RandomMoveInterval
        );

        await _gameRepository.CreateAsync(game, cancellationToken);

        return new CreateGameResponse(
            game.Id,
            game.BoardSize,
            game.WinCondition,
            game.FirstPlayer,
            game.CurrentPlayer,
            game.Status,
            game.CreatedAt
        );
    }
}
