# official .NET 9 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /source

# ensure .csproj files are present for restore
COPY *.sln .
COPY src/TicTacToe.Api/TicTacToe.Api.csproj ./src/TicTacToe.Api/
COPY src/TicTacToe.Domain/TicTacToe.Domain.csproj ./src/TicTacToe.Domain/
COPY src/TicTacToe.Application/TicTacToe.Application.csproj ./src/TicTacToe.Application/
COPY src/TicTacToe.Infrastructure/TicTacToe.Infrastructure.csproj ./src/TicTacToe.Infrastructure/
COPY tests/TicTacToe.Tests.Unit/TicTacToe.Tests.Unit.csproj ./tests/TicTacToe.Tests.Unit/
COPY tests/TicTacToe.Tests.Integration/TicTacToe.Tests.Integration.csproj ./tests/TicTacToe.Tests.Integration/

# restore dependencies (dependencies will be cached
# - unless project files change)
RUN dotnet restore

# copy source code
COPY src/ ./src/
COPY tests/ ./tests/

# build and publish API project
WORKDIR /source/src/TicTacToe.Api
RUN dotnet publish -c Release -o /app --no-restore

# final stage: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# set the working directory
WORKDIR /app

# install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# copy the published app from the build stage
COPY --from=build /app .

# create a non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1000 appuser && chown -R appuser /app
USER appuser

EXPOSE 8080

# set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# run asp.net Web API app
ENTRYPOINT ["dotnet", "TicTacToe.Api.dll"]
