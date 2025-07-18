using TicTacToe.Domain.Entities;
using TicTacToe.Domain.ValueObjects;
using Xunit;

namespace TicTacToe.Tests.Unit.Entities;

public class GameBoardTests
{
    [Fact]
    public void Constructor_ValidSize_CreatesBoard()
    {
        var board = new GameBoard(3);

        Assert.Equal(3, board.Size);
        Assert.True(board.IsPositionEmpty(new Position(0, 0)));
        Assert.True(board.IsPositionEmpty(new Position(2, 2)));
    }

    [Fact]
    public void Constructor_InvalidSize_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new GameBoard(2));
    }

    [Fact]
    public void PlaceSymbol_ValidPosition_PlacesSymbol()
    {
        var board = new GameBoard(3);
        var position = new Position(1, 1);

        board.PlaceSymbol(position, PlayerSymbol.X);

        Assert.Equal(PlayerSymbol.X, board[position]);
        Assert.False(board.IsPositionEmpty(position));
    }

    [Fact]
    public void PlaceSymbol_OccupiedPosition_ThrowsInvalidOperationException()
    {
        var board = new GameBoard(3);
        var position = new Position(1, 1);
        board.PlaceSymbol(position, PlayerSymbol.X);

        Assert.Throws<InvalidOperationException>(() => board.PlaceSymbol(position, PlayerSymbol.O));
    }

    [Fact]
    public void GetEmptyPositions_EmptyBoard_ReturnsAllPositions()
    {
        var board = new GameBoard(3);

        var emptyPositions = board.GetEmptyPositions();

        Assert.Equal(9, emptyPositions.Count);
        Assert.Contains(new Position(0, 0), emptyPositions);
        Assert.Contains(new Position(2, 2), emptyPositions);
    }

    [Fact]
    public void GetEmptyPositions_PartiallyFilled_ReturnsOnlyEmpty()
    {
        var board = new GameBoard(3);
        board.PlaceSymbol(new Position(0, 0), PlayerSymbol.X);
        board.PlaceSymbol(new Position(1, 1), PlayerSymbol.O);

        var emptyPositions = board.GetEmptyPositions();

        Assert.Equal(7, emptyPositions.Count);
        Assert.DoesNotContain(new Position(0, 0), emptyPositions);
        Assert.DoesNotContain(new Position(1, 1), emptyPositions);
        Assert.Contains(new Position(2, 2), emptyPositions);
    }

    [Fact]
    public void IsFull_EmptyBoard_ReturnsFalse()
    {
        var board = new GameBoard(3);
        var result = board.IsFull();
        Assert.False(result);
    }

    [Fact]
    public void IsFull_FullBoard_ReturnsTrue()
    {
        var board = new GameBoard(3);
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                board.PlaceSymbol(new Position(row, col), PlayerSymbol.X);
            }
        }

        var result = board.IsFull();
        Assert.True(result);
    }
}
