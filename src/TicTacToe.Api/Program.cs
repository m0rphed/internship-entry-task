using TicTacToe.Application.Extensions;
using TicTacToe.Infrastructure.Extensions;
using TicTacToe.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// add services: controllers, swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add application services (registered use cases)
builder.Services.AddApplication();

// add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// add CORS for the dev environment
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// ensure DB is created (use InMemory for development environment)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TicTacToeDbContext>();
    context.Database.EnsureCreated();
}

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}

// health check endpoint 
app.MapHealthChecks("/health");

app.UseRouting();
app.MapControllers();

app.Run();