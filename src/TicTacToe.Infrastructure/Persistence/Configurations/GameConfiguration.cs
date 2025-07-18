using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicTacToe.Domain.Entities;
using TicTacToe.Domain.ValueObjects;
using TicTacToe.Infrastructure.Persistence.Converters;

namespace TicTacToe.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(g => g.BoardSize)
            .IsRequired();

        builder.Property(g => g.WinCondition)
            .IsRequired();

        builder.Property(g => g.FirstPlayer)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.CurrentPlayer)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(g => g.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(g => g.RandomMoveChance)
            .IsRequired()
            .HasColumnType("double precision");

        builder.Property(g => g.RandomMoveInterval)
            .IsRequired();

        // the board property would be handled through moves
        // TODO: (DO NOT FORGER) handle `g.Board` properly
        builder.Ignore(g => g.Board);

        // set up the moves collection // = ходы в игре
        builder.OwnsMany(g => g.Moves, move =>
        {
            move.ToTable("Moves");

            move.WithOwner()
                .HasForeignKey("GameId");

            move.Property<int>("Id")
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            move.HasKey("Id");

            move.Property(m => m.Position)
                .HasConversion(new PositionConverter())
                .HasColumnName("Position")
                .IsRequired();

            move.Property(m => m.Symbol)
                .HasConversion<int>()
                .IsRequired();

            move.Property(m => m.MoveNumber)
                .IsRequired();

            move.Property(m => m.IsRandomMove)
                .IsRequired();

            move.Property(m => m.Timestamp)
                .IsRequired()
                .HasColumnType("timestamp with time zone");
        });

        // create indexes
        // TODO: not sure if we really need indexes in our schema
        builder.HasIndex(g => g.Status);
        builder.HasIndex(g => g.CreatedAt);
        builder.HasIndex(g => g.UpdatedAt);
    }
}