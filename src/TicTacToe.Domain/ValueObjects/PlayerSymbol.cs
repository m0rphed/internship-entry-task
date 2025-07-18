namespace TicTacToe.Domain.ValueObjects;

public enum PlayerSymbol
{
    X = 1,
    O = 2
}

public static class PlayerSymbolExtensions
{
    public static PlayerSymbol Opposite(this PlayerSymbol symbol) => symbol switch
    {
        PlayerSymbol.X => PlayerSymbol.O,
        PlayerSymbol.O => PlayerSymbol.X,
        _ => throw new ArgumentException($"Invalid player symbol: {symbol}")
    };

    public static string ToDisplayString(this PlayerSymbol symbol) => symbol switch
    {
        PlayerSymbol.X => "X",
        PlayerSymbol.O => "O",
        _ => throw new ArgumentException($"Invalid player symbol: {symbol}")
    };
}
