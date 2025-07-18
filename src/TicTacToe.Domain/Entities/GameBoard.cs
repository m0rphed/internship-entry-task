using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Domain.Entities;

public class GameBoard
{
    private readonly PlayerSymbol?[,] _board;

    public GameBoard(int size)
    {
        if (size < 3)
            throw new ArgumentException("Board size must be at least 3", nameof(size));

        Size = size;
        _board = new PlayerSymbol?[size, size];
    }

    public int Size { get; }

    public PlayerSymbol? this[int row, int column] => _board[row, column];

    public PlayerSymbol? this[Position position] => _board[position.Row, position.Column];

    public bool IsPositionEmpty(Position position)
    {
        if (!position.IsValid(Size))
            return false;

        return _board[position.Row, position.Column] is null;
    }

    public bool IsFull()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                if (_board[row, col] is null)
                    return false;
            }
        }
        return true;
    }

    public void PlaceSymbol(Position position, PlayerSymbol symbol)
    {
        if (!position.IsValid(Size))
            throw new ArgumentException($"Position {position} is not valid for board size {Size}");

        if (!IsPositionEmpty(position))
            throw new InvalidOperationException($"Position {position} is already occupied");

        _board[position.Row, position.Column] = symbol;
    }

    public List<Position> GetEmptyPositions()
    {
        var emptyPositions = new List<Position>();
        
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                if (_board[row, col] is null)
                {
                    emptyPositions.Add(new Position(row, col));
                }
            }
        }
        
        return emptyPositions;
    }

    public GameBoard Clone()
    {
        var clonedBoard = new GameBoard(Size);
        
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                if (_board[row, col] is not null)
                {
                    clonedBoard._board[row, col] = _board[row, col];
                }
            }
        }
        
        return clonedBoard;
    }

    public string ToDisplayString()
    {
        var result = new System.Text.StringBuilder();
        
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                var symbol = _board[row, col]?.ToDisplayString() ?? " ";
                result.Append(symbol);
                
                if (col < Size - 1)
                    result.Append(" | ");
            }

            if (row >= Size - 1) continue;
            result.AppendLine();
            result.AppendLine(new string('-', Size * 4 - 1));
        }
        
        return result.ToString();
    }
}
