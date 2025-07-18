using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Application.DTOs;

public record CreateGameRequest(
    int BoardSize = 3,
    int WinCondition = 3,
    PlayerSymbol FirstPlayer = PlayerSymbol.X,
    double RandomMoveChance = 0.1,
    int RandomMoveInterval = 3
);

public record CreateGameResponse(
    Guid Id,
    int BoardSize,
    int WinCondition,
    PlayerSymbol FirstPlayer,
    PlayerSymbol CurrentPlayer,
    GameStatus Status,
    DateTime CreatedAt
);

public record GetGameResponse(
    Guid Id,
    int BoardSize,
    int WinCondition,
    PlayerSymbol FirstPlayer,
    PlayerSymbol CurrentPlayer,
    GameStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MoveCount,
    IReadOnlyList<MoveDto> Moves,
    string? BoardDisplay
);

public record MoveDto(
    int MoveNumber,
    PlayerSymbol Symbol,
    int Row,
    int Column,
    bool IsRandomMove,
    DateTime Timestamp
);

public record MakeMoveRequest(
    int Row,
    int Column
);

public record MakeMoveResponse(
    Guid GameId,
    MoveDto Move,
    PlayerSymbol CurrentPlayer,
    GameStatus Status,
    string? BoardDisplay
);