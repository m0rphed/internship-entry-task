using TicTacToe.Domain.ValueObjects;
using Xunit;

namespace TicTacToe.Tests.Unit.ValueObjects;

public class PlayerSymbolTests
{
    [Fact]
    public void Opposite_X_ReturnsO()
    {
        var symbol = PlayerSymbol.X;
        var result = symbol.Opposite();
        Assert.Equal(PlayerSymbol.O, result);
    }

    [Fact]
    public void Opposite_O_ReturnsX()
    {
        var symbol = PlayerSymbol.O;

        var result = symbol.Opposite();

        Assert.Equal(PlayerSymbol.X, result);
    }

    [Theory]
    [InlineData(PlayerSymbol.X, "X")]
    [InlineData(PlayerSymbol.O, "O")]
    public void ToDisplayString_ReturnsCorrectString(PlayerSymbol symbol, string expected)
    {
        var result = symbol.ToDisplayString();

        Assert.Equal(expected, result);
    }
}
