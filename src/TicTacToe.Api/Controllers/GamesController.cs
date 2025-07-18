using Microsoft.AspNetCore.Mvc;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly ILogger<GamesController> _logger;

    public GamesController(ILogger<GamesController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<string> Get()
    {
        _logger.LogInformation("GET /api/games called");
        return Ok("TicTacToe API is running!");
    }

    [HttpGet("{id}")]
    public ActionResult<string> Get(Guid id)
    {
        _logger.LogInformation("GET /api/games/{GameId} called", id);
        return Ok($"Game {id} - placeholder response");
    }

    [HttpPost]
    public ActionResult<string> Post()
    {
        _logger.LogInformation("POST /api/games called");
        return Ok("New game created - placeholder response");
    }

    [HttpPost("{id}/moves")]
    public ActionResult<string> Move(Guid id, [FromBody] object move)
    {
        _logger.LogInformation("POST /api/games/{GameId}/moves called", id);
        return Ok($"Move made in game {id} - placeholder response");
    }
}
