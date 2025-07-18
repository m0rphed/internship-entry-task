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
        return await _context.Games
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
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
        return await _context.Games
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Game>> GetByStatusAsync(GameStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .Where(g => g.Status == status)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}