namespace TicTacToe.Domain.ValueObjects;

public enum GameStatus
{
    NotStarted = 0,
    InProgress = 1,
    WonByPlayerX = 2,
    WonByPlayerO = 3,
    Draw = 4
}

public static class GameStatusExtensions
{
    public static bool IsFinished(this GameStatus status) => status switch
    {
        GameStatus.WonByPlayerX => true,
        GameStatus.WonByPlayerO => true,
        GameStatus.Draw => true,
        _ => false
    };

    public static PlayerSymbol? Winner(this GameStatus status) => status switch
    {
        GameStatus.WonByPlayerX => PlayerSymbol.X,
        GameStatus.WonByPlayerO => PlayerSymbol.O,
        _ => null
    };

    public static string ToDisplayString(this GameStatus status) => status switch
    {
        GameStatus.NotStarted => "Not Started",
        GameStatus.InProgress => "In Progress",
        GameStatus.WonByPlayerX => "Won by X",
        GameStatus.WonByPlayerO => "Won by O",
        GameStatus.Draw => "Draw",
        _ => throw new ArgumentException($"Invalid game status: {status}")
    };
}
