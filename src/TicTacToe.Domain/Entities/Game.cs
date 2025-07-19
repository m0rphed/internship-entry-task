using TicTacToe.Domain.Services;
using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Domain.Entities;

public class Game
{
    private readonly List<Move> _moves;
    private GameBoard _board;
    private readonly WinConditionChecker _winConditionChecker;
    private readonly RandomMoveService _randomMoveService;

    // public constructor for creating new games
    public Game(
        Guid id,
        int boardSize,
        int winCondition,
        PlayerSymbol firstPlayer = PlayerSymbol.X,
        double randomMoveChance = 0.1,
        int randomMoveInterval = 3)
    {
        if (boardSize < 3)
        {
            throw new ArgumentException("Board size must be at least 3", nameof(boardSize));
        }

        if (winCondition < 3 || winCondition > boardSize)
        {
            throw new ArgumentException("Win condition must be between 3 and board size", nameof(winCondition));
        }

        Id = id;
        BoardSize = boardSize;
        WinCondition = winCondition;
        FirstPlayer = firstPlayer;
        CurrentPlayer = firstPlayer;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RandomMoveChance = randomMoveChance;
        RandomMoveInterval = randomMoveInterval;

        _moves = new List<Move>();
        _board = new GameBoard(boardSize);
        _winConditionChecker = new WinConditionChecker(winCondition);
        _randomMoveService = new RandomMoveService(randomMoveChance, randomMoveInterval);

        Status = GameStatus.InProgress;
    }

    // private constructor for EF
    // TODO: find better solution for this
    private Game(
        Guid id,
        int boardSize,
        int winCondition,
        PlayerSymbol firstPlayer,
        PlayerSymbol currentPlayer,
        GameStatus status,
        DateTime createdAt,
        DateTime updatedAt,
        double randomMoveChance,
        int randomMoveInterval)
    {
        Id = id;
        BoardSize = boardSize;
        WinCondition = winCondition;
        FirstPlayer = firstPlayer;
        CurrentPlayer = currentPlayer;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        RandomMoveChance = randomMoveChance;
        RandomMoveInterval = randomMoveInterval;

        _moves = new List<Move>();
        _board = new GameBoard(boardSize);
        _winConditionChecker = new WinConditionChecker(winCondition);
        _randomMoveService = new RandomMoveService(randomMoveChance, randomMoveInterval);
    }

    /// <summary>
    /// Reconstructs the board state from moves
    /// (used by EF Core after loading from database)
    /// </summary>
    public void ReconstructBoardFromMoves()
    {
        _board = new GameBoard(BoardSize);
        foreach (var move in _moves.OrderBy(m => m.MoveNumber))
        {
            _board.PlaceSymbol(move.Position, move.Symbol);
        }
    }

    public Guid Id { get; }
    public int BoardSize { get; }
    public int WinCondition { get; }
    public PlayerSymbol FirstPlayer { get; }
    public PlayerSymbol CurrentPlayer { get; private set; }
    public GameStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public double RandomMoveChance { get; }
    public int RandomMoveInterval { get; }

    public IReadOnlyList<Move> Moves => _moves.AsReadOnly();
    public GameBoard Board => _board;
    public int MoveCount => _moves.Count;

    /// <summary>
    /// Attempts to make a move at the specified position
    /// </summary>
    /// <param name="position">Position to place the symbol</param>
    /// <returns>The (actual) move that was made (might be different due to random move logic)</returns>
    public Move MakeMove(Position position)
    {
        ValidateMove(position);

        var moveNumber = _moves.Count + 1;
        var intendedSymbol = CurrentPlayer;
        var actualMove = CreateMove(position, intendedSymbol, moveNumber);

        ApplyMove(actualMove);
        UpdateGameState();

        return actualMove;
    }

    private void ValidateMove(Position position)
    {
        if (Status.IsFinished())
        {
            throw new InvalidOperationException("Cannot make a move in a finished game");
        }

        if (!position.IsValid(BoardSize))
        {
            throw new ArgumentException($"Position {position} is not valid for board size {BoardSize}");
        }

        if (!_board.IsPositionEmpty(position))
        {
            throw new InvalidOperationException($"Position {position} is already occupied");
        }
    }

    private Move CreateMove(Position position, PlayerSymbol intendedSymbol, int moveNumber)
    {
        // check if random move should be applied
        if (_randomMoveService.ShouldApplyRandomMove(moveNumber))
        {
            var randomPosition = _randomMoveService.GetRandomEmptyPosition(_board);
            if (randomPosition != null)
            {
                var randomSymbol = RandomMoveService.GetRandomMoveSymbol(intendedSymbol);
                return new Move(randomPosition, randomSymbol, moveNumber, isRandomMove: true);
            }
        }

        // make a move
        return new Move(position, intendedSymbol, moveNumber, isRandomMove: false);
    }

    private void ApplyMove(Move move)
    {
        _board.PlaceSymbol(move.Position, move.Symbol);
        _moves.Add(move);
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateGameState()
    {
        // check game status
        Status = _winConditionChecker.CheckGameStatus(_board);

        // update the player if game is still in progress
        if (Status == GameStatus.InProgress)
        {
            CurrentPlayer = CurrentPlayer.Opposite();
        }
    }

    public Move? GetLastMove() => _moves.LastOrDefault();

    public bool IsPlayerTurn(PlayerSymbol player) => CurrentPlayer == player && Status == GameStatus.InProgress;

    public override string ToString()
    {
        return
            $"Game {Id}: {Status.ToDisplayString()}, Move {MoveCount}, Current Player: {CurrentPlayer.ToDisplayString()}";
    }
}