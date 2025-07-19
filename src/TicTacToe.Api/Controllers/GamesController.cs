using Microsoft.AspNetCore.Mvc;
using TicTacToe.Application.DTOs;
using TicTacToe.Application.UseCases;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly CreateGameUseCase _createGameUseCase;
    private readonly GetGameUseCase _getGameUseCase;
    private readonly MakeMoveUseCase _makeMoveUseCase;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        CreateGameUseCase createGameUseCase,
        GetGameUseCase getGameUseCase,
        MakeMoveUseCase makeMoveUseCase,
        ILogger<GamesController> logger)
    {
        _createGameUseCase = createGameUseCase ?? throw new ArgumentNullException(nameof(createGameUseCase));
        _getGameUseCase = getGameUseCase ?? throw new ArgumentNullException(nameof(getGameUseCase));
        _makeMoveUseCase = makeMoveUseCase ?? throw new ArgumentNullException(nameof(makeMoveUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<ActionResult<CreateGameResponse>> CreateGame(
        [FromBody] CreateGameRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new game with board size {BoardSize}", request.BoardSize);
            var response = await _createGameUseCase.ExecuteAsync(request, cancellationToken);
            _logger.LogInformation("Game created successfully with ID {GameId}", response.Id);

            return CreatedAtAction(nameof(GetGame), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid game parameters");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetGameResponse>> GetGame(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting game with ID {GameId}", id);
            var response = await _getGameUseCase.ExecuteAsync(id, cancellationToken);
            if (response != null) return Ok(response);

            _logger.LogWarning("Game not found with ID {GameId}", id);
            return NotFound(new { error = "Game not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game with ID {GameId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("{id}/moves")]
    public async Task<ActionResult<MakeMoveResponse>> MakeMove(
        Guid id,
        [FromBody] MakeMoveRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Making move in game {GameId} at position ({Row}, {Column})",
                id, request.Row, request.Column);

            var response = await _makeMoveUseCase.ExecuteAsync(id, request, cancellationToken);

            if (response == null)
            {
                _logger.LogWarning("Game not found with ID {GameId}", id);
                return NotFound(new { error = "Game not found" });
            }

            _logger.LogInformation("Move made successfully in game {GameId}", id);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already occupied"))
        {
            _logger.LogWarning(ex, "Position already occupied in game {GameId}", id);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("finished game"))
        {
            _logger.LogWarning(ex, "Attempted move on finished game {GameId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for game {GameId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid move parameters for game {GameId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making move in game {GameId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}