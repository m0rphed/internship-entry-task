using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using FluentAssertions;
using TicTacToe.Application.DTOs;
using TicTacToe.Domain.ValueObjects;
using Testcontainers.PostgreSql;
using TicTacToe.Application.Extensions;
using TicTacToe.Infrastructure.Extensions;
using TicTacToe.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Xunit.Abstractions;

namespace TicTacToe.Tests.Integration;

public class TicTacToeApiIntegrationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("TicTacToe")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private TestServer _server = null!;
    private HttpClient _client = null!;

    public TicTacToeApiIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection",
                    _postgresContainer.GetConnectionString()),
                new KeyValuePair<string, string?>("TicTacToe:BoardSize", "3"),
                new KeyValuePair<string, string?>("TicTacToe:WinCondition", "3"),
                new KeyValuePair<string, string?>("TicTacToe:RandomMoveChance", "0.1"),
                new KeyValuePair<string, string?>("TicTacToe:RandomMoveInterval", "3")
            ])
            .Build();

        var webHostBuilder = new WebHostBuilder()
            .UseConfiguration(configuration)
            .ConfigureServices(services =>
            {
                services.AddControllers()
                    .AddApplicationPart(typeof(Api.Controllers.GamesController).Assembly);
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();
                services.AddApplication();
                services.AddInfrastructure(configuration);
                services.AddHealthChecks();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHealthChecks("/health");
                });
            });

        _server = new TestServer(webHostBuilder);
        _client = _server.CreateClient();

        // ensure database is created
        using var scope = _server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TicTacToeDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        _server.Dispose();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn_Healthy()
    {
        // act
        var response = await _client.GetAsync("/health");

        // [debug]
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine($"Health Status: {statusCode}");
        _testOutputHelper.WriteLine($"Health Content: {content}");

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task Routes_ShouldBeDiscoverable()
    {
        // [debug] list ALL registered services
        using var scope = _server.Services.CreateScope();
        var services = scope.ServiceProvider.GetServices<object>().ToList();
        _testOutputHelper.WriteLine($"Total services registered: {services.Count}");

        // check if controllers are registered
        var controllerServices = _server.Services
            .GetService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();
        if (controllerServices != null)
        {
            var actions = controllerServices.ActionDescriptors.Items;
            _testOutputHelper.WriteLine($"Total actions found: {actions.Count}");
            foreach (var action in actions)
            {
                _testOutputHelper.WriteLine($"Action: {action.DisplayName}");
            }
        }
        else
        {
            _testOutputHelper.WriteLine("IActionDescriptorCollectionProvider not found!");
        }

        // test if controllers are discovered
        var response = await _client.GetAsync("/api/games");
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();

        _testOutputHelper.WriteLine($"GET /api/games Status: {statusCode}");
        _testOutputHelper.WriteLine($"GET /api/games Content: {content}");

        // expected: '405 Method Not Allowed' - because GET is not implemented 
        // or 404 if routing is broken
        statusCode.Should().NotBe(System.Net.HttpStatusCode.NotFound, "Controllers should be discoverable");
    }

    [Fact]
    public async Task CreateGame_ShouldCreateNewGame_WithCorrectDefaults()
    {
        // arrange
        var request = new CreateGameRequest(
            BoardSize: 3,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 0.1,
            RandomMoveInterval: 3
        );

        // act
        var response = await _client.PostAsJsonAsync("/api/games", request);

        // [debug]
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine($"Status: {statusCode}");
        _testOutputHelper.WriteLine($"Content: {content}");

        // assert
        response.IsSuccessStatusCode.Should().BeTrue($"Expected success but got {statusCode}. Content: {content}");
        var game = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

        game.Should().NotBeNull();
        game.BoardSize.Should().Be(3);
        game.WinCondition.Should().Be(3);
        game.FirstPlayer.Should().Be(PlayerSymbol.X);
        game.CurrentPlayer.Should().Be(PlayerSymbol.X);
        game.Status.Should().Be(GameStatus.InProgress);
        game.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task MakeMove_ShouldUpdateGameState()
    {
        // arrange: create game
        var createRequest = new CreateGameRequest(
            BoardSize: 3,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 0.0, // no random moves for predictable testing
            RandomMoveInterval: 3
        );

        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // act: make first move
        var moveRequest = new MakeMoveRequest(Row: 0, Column: 0);
        var moveResponse = await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves", moveRequest);

        // assert
        moveResponse.IsSuccessStatusCode.Should().BeTrue();
        var moveResult = await moveResponse.Content.ReadFromJsonAsync<MakeMoveResponse>();

        moveResult.Should().NotBeNull();
        moveResult.Move.Symbol.Should().Be(PlayerSymbol.X);
        moveResult.Move.Row.Should().Be(0);
        moveResult.Move.Column.Should().Be(0);
        moveResult.Move.MoveNumber.Should().Be(1);
        moveResult.Move.IsRandomMove.Should().BeFalse();
        moveResult.CurrentPlayer.Should().Be(PlayerSymbol.O); // next player
        moveResult.Status.Should().Be(GameStatus.InProgress);
    }

    [Fact]
    public async Task SpecialRandomMove_Every3rdMove_ShouldTriggerRandomMove()
    {
        // arrange: create game with 100% random chance for testing
        var createRequest = new CreateGameRequest(
            BoardSize: 3,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 1.0, // 100% chance for testing
            RandomMoveInterval: 3
        );

        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // act: make 3 simple moves
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 1));
        var move3Response = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 2));

        // assert: third move should be random
        var move3Result = await move3Response.Content.ReadFromJsonAsync<MakeMoveResponse>();
        move3Result.Should().NotBeNull();
        move3Result.Move.IsRandomMove.Should().BeTrue();
        move3Result.Move.MoveNumber.Should().Be(3);
    }

    [Fact]
    public async Task GetGame_ShouldReturnGameWithMoves()
    {
        // arrange: create game and make a move
        var createRequest = new CreateGameRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        var moveRequest = new MakeMoveRequest(Row: 1, Column: 1);
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves", moveRequest);

        // act
        var getResponse = await _client.GetAsync($"/api/games/{game.Id}");

        // assert
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var gameDetails = await getResponse.Content.ReadFromJsonAsync<GetGameResponse>();

        gameDetails.Should().NotBeNull();
        gameDetails.Id.Should().Be(game.Id);
        gameDetails.MoveCount.Should().Be(1);
        gameDetails.Moves.Should().HaveCount(1);
        gameDetails.Moves[0].Row.Should().Be(1);
        gameDetails.Moves[0].Column.Should().Be(1);
        gameDetails.Moves[0].Symbol.Should().Be(PlayerSymbol.X);
    }

    [Fact]
    public async Task CreateGame_WithCustomParameters_ShouldUseProvidedValues()
    {
        // arrange
        var request = new CreateGameRequest(
            BoardSize: 5,
            WinCondition: 4,
            FirstPlayer: PlayerSymbol.O,
            RandomMoveChance: 0.2,
            RandomMoveInterval: 5
        );

        // act
        var response = await _client.PostAsJsonAsync("/api/games", request);

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var game = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

        game.Should().NotBeNull();
        game.BoardSize.Should().Be(5);
        game.WinCondition.Should().Be(4);
        game.FirstPlayer.Should().Be(PlayerSymbol.O);
        game.CurrentPlayer.Should().Be(PlayerSymbol.O);
    }

    [Fact]
    public async Task CreateGame_WithInvalidParameters_ShouldReturnBadRequest()
    {
        // arrange: invalid board size (too small)
        var request = new CreateGameRequest(
            BoardSize: 2, // < 3, when 3 is MIN
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 0.1,
            RandomMoveInterval: 3
        );

        // act
        var response = await _client.PostAsJsonAsync("/api/games", request);

        // assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MakeMove_OnSamePosition_ShouldReturnConflict()
    {
        // arrange: create game and make first move
        var createRequest = new CreateGameRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0));

        // act: try to make move on same position
        var moveResponse = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0));

        // assert
        moveResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MakeMove_OutOfBounds_ShouldReturnBadRequest()
    {
        // arrange
        var createRequest = new CreateGameRequest(BoardSize: 3);
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // act: try to make move outside board bounds
        var moveResponse = await _client.PostAsJsonAsync(
            $"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 5, Column: 5));
        // assert
        moveResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetGame_NonExistentGame_ShouldReturnNotFound()
    {
        // arrange
        var nonExistentId = Guid.NewGuid();
        // act
        var response = await _client.GetAsync($"/api/games/{nonExistentId}");
        // assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MakeMove_NonExistentGame_ShouldReturnNotFound()
    {
        // arrange
        var nonExistentId = Guid.NewGuid();
        var moveRequest = new MakeMoveRequest(Row: 0, Column: 0);

        // act
        var response = await _client.PostAsJsonAsync($"/api/games/{nonExistentId}/moves", moveRequest);

        // assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GameFlow_CompleteGame_ShouldDetectWinner()
    {
        // arrange: create 3x3 game
        var createRequest = new CreateGameRequest(
            BoardSize: 3,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 0.0, // no random moves
            RandomMoveInterval: 3
        );
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // act: play winning sequence for X
        // X plays: (0,0), (0,1), (0,2) for horizontal win
        // O plays: (1,0), (1,1) to block

        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0)); // X
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 1, Column: 0)); // O
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 1)); // X
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 1, Column: 1)); // O
        var winningMoveResponse = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 2)); // X wins

        // assert
        winningMoveResponse.IsSuccessStatusCode.Should().BeTrue();
        var winResult = await winningMoveResponse.Content.ReadFromJsonAsync<MakeMoveResponse>();
        winResult.Should().NotBeNull();
        winResult.Status.Should().Be(GameStatus.WonByPlayerX);
    }

    [Fact]
    public async Task GameFlow_FullBoard_ShouldDetectDraw()
    {
        // arrange: create 3x3 game
        var createRequest = new CreateGameRequest(
            BoardSize: 3,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 0.0, // no random moves
            RandomMoveInterval: 3
        );
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // act: play moves that result in draw
        // Final board will be:
        // X O X
        // O X X  
        // O X O
        var moves = new[]
        {
            (0, 0), // X - move 1
            (0, 1), // O - move 2 
            (0, 2), // X - move 3
            (1, 0), // O - move 4
            (1, 1), // X - move 5
            (2, 0), // O - move 6
            (1, 2), // X - move 7
            (2, 2), // O - move 8
            (2, 1) // X - move 9, board full, no winner
        };

        MakeMoveResponse? lastResult = null;
        foreach (var (row, col) in moves)
        {
            var response = await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
                new MakeMoveRequest(Row: row, Column: col));
            lastResult = await response.Content.ReadFromJsonAsync<MakeMoveResponse>();
        }

        // assert
        lastResult.Should().NotBeNull();
        lastResult.Status.Should().Be(GameStatus.Draw);
    }

    [Fact]
    public async Task MakeMove_OnFinishedGame_ShouldReturnBadRequest()
    {
        // arrange - create and finish a game
        var createRequest = new CreateGameRequest(
            BoardSize: 3,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 0.0,
            RandomMoveInterval: 3
        );
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // complete a winning sequence
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 1, Column: 0));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 1));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 1, Column: 1));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 2)); // X wins

        // act: try to make another move
        var response = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 2, Column: 0));

        // assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RandomMove_ShouldPlaceOpponentSymbol()
    {
        // arrange: create game with 100% random chance
        var createRequest = new CreateGameRequest(
            BoardSize: 4,
            WinCondition: 3,
            FirstPlayer: PlayerSymbol.X,
            RandomMoveChance: 1.0, // 100% chance
            RandomMoveInterval: 3
        );
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // act: make moves to trigger random move on 3rd turn
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0)); // X
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 1)); // O
        var randomMoveResponse = await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 2)); // should be random move

        // assert
        var result = await randomMoveResponse.Content.ReadFromJsonAsync<MakeMoveResponse>();
        result.Should().NotBeNull();
        result.Move.IsRandomMove.Should().BeTrue();
        result.Move.Symbol.Should().Be(PlayerSymbol.O); // opponent symbol (O instead of X)
        result.Move.MoveNumber.Should().Be(3);
        // position might be different from requested due to random placement
    }

    [Fact]
    public async Task PersistenceTest_GameShouldPersistBetweenRequests()
    {
        // arrange: create game and make moves
        var createRequest = new CreateGameRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/games", createRequest);
        var game = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // make moves
        await _client.PostAsJsonAsync($"/api/games/{game!.Id}/moves",
            new MakeMoveRequest(Row: 0, Column: 0));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 1, Column: 1));
        await _client.PostAsJsonAsync($"/api/games/{game.Id}/moves",
            new MakeMoveRequest(Row: 2, Column: 2));

        // act: retrieve game after moves
        var getResponse = await _client.GetAsync($"/api/games/{game.Id}");

        // assert: all data should be persisted
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var gameDetails = await getResponse.Content.ReadFromJsonAsync<GetGameResponse>();

        gameDetails.Should().NotBeNull();
        gameDetails.MoveCount.Should().Be(3);
        gameDetails.Moves.Should().HaveCount(3);
        gameDetails.CurrentPlayer.Should().Be(PlayerSymbol.O); // should be O's turn
        gameDetails.Status.Should().Be(GameStatus.InProgress);

        // check move sequence
        gameDetails.Moves[0].Symbol.Should().Be(PlayerSymbol.X);
        gameDetails.Moves[0].MoveNumber.Should().Be(1);
        gameDetails.Moves[1].Symbol.Should().Be(PlayerSymbol.O);
        gameDetails.Moves[1].MoveNumber.Should().Be(2);
        gameDetails.Moves[2].Symbol.Should().Be(PlayerSymbol.X);
        gameDetails.Moves[2].MoveNumber.Should().Be(3);
    }
}