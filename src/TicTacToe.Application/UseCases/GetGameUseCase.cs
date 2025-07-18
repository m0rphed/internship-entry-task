using TicTacToe.Application.DTOs;
using TicTacToe.Domain.Interfaces;

namespace TicTacToe.Application.UseCases;

public class GetGameUseCase
{
    private readonly IGameRepository _gameRepository;

    public GetGameUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<GetGameResponse?> ExecuteAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);

        if (game == null)
        {
            return null;
        }

        var moveDtos = game.Moves.Select(m => new MoveDto(
            m.MoveNumber,
            m.Symbol,
            m.Position.Row,
            m.Position.Column,
            m.IsRandomMove,
            m.Timestamp
        )).ToList();

        return new GetGameResponse(
            game.Id,
            game.BoardSize,
            game.WinCondition,
            game.FirstPlayer,
            game.CurrentPlayer,
            game.Status,
            game.CreatedAt,
            game.UpdatedAt,
            game.MoveCount,
            moveDtos,
            game.Board.ToDisplayString()
        );
    }
}