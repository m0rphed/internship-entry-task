using TicTacToe.Domain.ValueObjects;
using Xunit;

namespace TicTacToe.Tests.Unit.ValueObjects;

public class PositionTests
{
    [Fact]
    public void Create_ValidPosition_ReturnsPosition()
    {
        var position = Position.Create(1, 2);

        Assert.Equal(1, position.Row);
        Assert.Equal(2, position.Column);
    }

    [Fact]
    public void Create_NegativeRow_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Position.Create(-1, 2));
    }

    [Fact]
    public void Create_NegativeColumn_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Position.Create(1, -2));
    }

    [Theory]
    [InlineData(0, 0, 3, true)]
    [InlineData(2, 2, 3, true)]
    [InlineData(3, 2, 3, false)]
    [InlineData(2, 3, 3, false)]
    [InlineData(-1, 2, 3, false)]
    [InlineData(1, -1, 3, false)]
    public void IsValid_VariousPositions_ReturnsExpectedResult(int row, int col, int boardSize, bool expected)
    {
        var position = new Position(row, col);
        var result = position.IsValid(boardSize);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var position = new Position(1, 2);
        var result = position.ToString();
        Assert.Equal("(1, 2)", result);
    }
}
