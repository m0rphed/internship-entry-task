using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Domain.Entities;

public class Move
{
    public Move(Position position, PlayerSymbol symbol, int moveNumber, bool isRandomMove = false)
    {
        Position = position ?? throw new ArgumentNullException(nameof(position));
        Symbol = symbol;
        MoveNumber = moveNumber;
        IsRandomMove = isRandomMove;
        Timestamp = DateTime.UtcNow;
    }

    public Position Position { get; }
    public PlayerSymbol Symbol { get; }
    public int MoveNumber { get; }
    public bool IsRandomMove { get; }
    public DateTime Timestamp { get; }

    public override string ToString()
    {
        var randomIndicator = IsRandomMove ? " (Random)" : "";
        return $"Move {MoveNumber}: {Symbol.ToDisplayString()} at {Position}{randomIndicator}";
    }
}