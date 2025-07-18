using TicTacToe.Domain.Entities;
using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Domain.Services;

public class WinConditionChecker
{
    private readonly int _winCondition;

    public WinConditionChecker(int winCondition)
    {
        if (winCondition < 3)
            throw new ArgumentException("Win condition must be at least 3", nameof(winCondition));

        _winCondition = winCondition;
    }

    public GameStatus CheckGameStatus(GameBoard board)
    {
        // check for winner
        var winner = CheckForWinner(board);
        if (winner.HasValue)
        {
            return winner.Value == PlayerSymbol.X ? GameStatus.WonByPlayerX : GameStatus.WonByPlayerO;
        }

        // check for draw
        if (board.IsFull())
        {
            return GameStatus.Draw;
        }

        // game is still in progress
        return GameStatus.InProgress;
    }

    private PlayerSymbol? CheckForWinner(GameBoard board)
    {
        // check rows
        for (int row = 0; row < board.Size; row++)
        {
            var winner = CheckLine(board, row, 0, 0, 1);
            if (winner.HasValue) return winner;
        }

        // check columns
        for (int col = 0; col < board.Size; col++)
        {
            var winner = CheckLine(board, 0, col, 1, 0);
            if (winner.HasValue) return winner;
        }

        // check diagonals (top-left to bottom-right)
        for (int startRow = 0; startRow <= board.Size - _winCondition; startRow++)
        {
            for (int startCol = 0; startCol <= board.Size - _winCondition; startCol++)
            {
                var winner = CheckLine(board, startRow, startCol, 1, 1);
                if (winner.HasValue) return winner;
            }
        }

        // check diagonals (top-right to bottom-left)
        for (int startRow = 0; startRow <= board.Size - _winCondition; startRow++)
        {
            for (int startCol = _winCondition - 1; startCol < board.Size; startCol++)
            {
                var winner = CheckLine(board, startRow, startCol, 1, -1);
                if (winner.HasValue) return winner;
            }
        }

        return null;
    }

    private PlayerSymbol? CheckLine(GameBoard board, int startRow, int startCol, int deltaRow, int deltaCol)
    {
        var symbols = new List<PlayerSymbol?>();
        
        for (int i = 0; i < _winCondition; i++)
        {
            var row = startRow + i * deltaRow;
            var col = startCol + i * deltaCol;
            
            if (row < 0 || row >= board.Size || col < 0 || col >= board.Size)
                return null;
                
            symbols.Add(board[row, col]);
        }

        // check if all symbols are the same && not null
        var firstSymbol = symbols[0];
        if (firstSymbol.HasValue && symbols.All(s => s == firstSymbol))
        {
            return firstSymbol;
        }

        return null;
    }
}
