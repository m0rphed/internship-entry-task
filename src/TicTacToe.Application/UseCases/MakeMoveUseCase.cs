using TicTacToe.Application.DTOs;
using TicTacToe.Domain.Interfaces;
using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Application.UseCases;

public class MakeMoveUseCase
{
    private readonly IGameRepository _gameRepository;

    public MakeMoveUseCase(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<MakeMoveResponse?> ExecuteAsync(
        Guid gameId,
        MakeMoveRequest request,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);

        if (game == null)
        {
            return null;
        }

        try
        {
            var position = Position.Create(request.Row, request.Column);
            var actualMove = game.MakeMove(position);

            await _gameRepository.UpdateAsync(game, cancellationToken);

            var moveDto = new MoveDto(
                actualMove.MoveNumber,
                actualMove.Symbol,
                actualMove.Position.Row,
                actualMove.Position.Column,
                actualMove.IsRandomMove,
                actualMove.Timestamp
            );

            return new MakeMoveResponse(
                game.Id,
                moveDto,
                game.CurrentPlayer,
                game.Status,
                game.Board.ToDisplayString()
            );
        }
        // TODO: implement exception catch-cases 
        catch (ArgumentException)
        {
            // position validation errors - rethrow for controller to handle
            throw;
        }
        catch (InvalidOperationException)
        {
            // game state validation errors - rethrow for controller to handle  
            throw;
        }
    }
}