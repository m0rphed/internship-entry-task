var builder = WebApplication.CreateBuilder(args);

// add services: controllers, swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add health checks
builder.Services.AddHealthChecks();

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
