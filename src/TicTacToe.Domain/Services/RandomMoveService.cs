using TicTacToe.Domain.Entities;
using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Domain.Services;

public class RandomMoveService
{
    private readonly Random _random;
    private readonly double _randomMoveChance;
    private readonly int _randomMoveInterval;

    public RandomMoveService(double randomMoveChance = 0.1, int randomMoveInterval = 3, Random? random = null)
    {
        if (randomMoveChance is < 0 or > 1)
        {
            throw new ArgumentException("Random move chance must be between 0 and 1", nameof(randomMoveChance));
        }

        if (randomMoveInterval < 1)
        {
            throw new ArgumentException("Random move interval must be at least 1", nameof(randomMoveInterval));
        }

        _randomMoveChance = randomMoveChance;
        _randomMoveInterval = randomMoveInterval;
        _random = random ?? new Random();
    }

    /// <summary>
    /// Checks if a random move should occur based on the move number and probability
    /// </summary>
    /// <param name="moveNumber">Current move number (1-based)</param>
    /// <returns>True if a random move should occur</returns>
    public bool ShouldApplyRandomMove(int moveNumber)
    {
        // check if this is the interval move (every 3rd move)
        if (moveNumber % _randomMoveInterval != 0)
        {
            return false;
        }

        // roll for the probability (10% chance)
        return _random.NextDouble() < _randomMoveChance;
    }

    /// <summary>
    /// Selects a random empty position on the board
    /// </summary>
    /// <param name="board">The game board</param>
    /// <returns>Random empty position, or null if board is full</returns>
    public Position? GetRandomEmptyPosition(GameBoard board)
    {
        var emptyPositions = board.GetEmptyPositions();

        if (emptyPositions.Count == 0)
        {
            return null;
        }

        var randomIndex = _random.Next(emptyPositions.Count);
        return emptyPositions[randomIndex];
    }

    /// <summary>
    /// Determines the symbol for a random move (opposite of the intended player)
    /// </summary>
    /// <param name="intendedSymbol">The symbol the player intended to place</param>
    /// <returns>The opposite symbol</returns>
    public static PlayerSymbol GetRandomMoveSymbol(PlayerSymbol intendedSymbol)
    {
        return intendedSymbol.Opposite();
    }
}