namespace TicTacToe.Domain.ValueObjects;

public record Position(int Row, int Column)
{
    public static Position Create(int row, int column)
    {
        if (row < 0)
        {
            throw new ArgumentException("Row cannot be negative", nameof(row));
        }

        if (column < 0)
        {
            throw new ArgumentException("Column cannot be negative", nameof(column));
        }

        return new Position(row, column);
    }

    public bool IsValid(int boardSize) => 
        Row >= 0 && Row < boardSize && 
        Column >= 0 && Column < boardSize;

    public override string ToString() => $"({Row}, {Column})";
}
