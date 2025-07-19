using Microsoft.EntityFrameworkCore;
using TicTacToe.Domain.Entities;
using TicTacToe.Domain.Interfaces;
using TicTacToe.Domain.ValueObjects;
using TicTacToe.Infrastructure.Persistence;

namespace TicTacToe.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly TicTacToeDbContext _context;

    public GameRepository(TicTacToeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await _context.Games
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        // reconstruct board state from moves if game exists
        game?.ReconstructBoardFromMoves();
        
        return game;
    }

    public async Task<Game> CreateAsync(Game game, CancellationToken cancellationToken = default)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync(cancellationToken);
        return game;
    }

    public async Task<Game> UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync(cancellationToken);
        return game;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .AnyAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var games = await _context.Games
            .Include(g => g.Moves)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);

        // Reconstruct board state for all games
        foreach (var game in games)
        {
            game.ReconstructBoardFromMoves();
        }
        
        return games;
    }

    public async Task<IEnumerable<Game>> GetByStatusAsync(GameStatus status,
        CancellationToken cancellationToken = default)
    {
        var games = await _context.Games
            .Include(g => g.Moves)
            .Where(g => g.Status == status)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);

        // Reconstruct board state for all games
        foreach (var game in games)
        {
            game.ReconstructBoardFromMoves();
        }
        
        return games;
    }
}