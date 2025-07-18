using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicTacToe.Domain.ValueObjects;

namespace TicTacToe.Infrastructure.Persistence.Converters;

// converter for tic-tac-toe positions
public class PositionConverter : ValueConverter<Position, string>
{
    public PositionConverter() : base(
        position => $"{position.Row},{position.Column}",
        value => ParsePosition(value))
    {
    }

    private static Position ParsePosition(string value)
    {
        var parts = value.Split(',');
        return new Position(int.Parse(parts[0]), int.Parse(parts[1]));
    }
}