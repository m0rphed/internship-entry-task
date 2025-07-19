using TicTacToe.Api;

var builder = WebApplication.CreateBuilder(args);
var app = WebApplicationConfigurator.ConfigureWebApplication(builder);

app.Run();