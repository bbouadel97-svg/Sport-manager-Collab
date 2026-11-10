using Microsoft.EntityFrameworkCore;
using TeamManager.Models;

namespace TeamManager.Data;

public class SportManagerDbContext(DbContextOptions<SportManagerDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Injury> Injuries => Set<Injury>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Team)
            .WithMany(t => t.Players)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.Position)
            .WithMany(pos => pos.Players)
            .HasForeignKey(p => p.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Injury>()
            .HasOne(i => i.Player)
            .WithMany(p => p.Injuries)
            .HasForeignKey(i => i.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany(t => t.HomeMatches)
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany(t => t.AwayMatches)
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Position>().HasData(
            new Position { Id = 1, Name = "Goalkeeper", NeededSpeed = 55, NeededEndurance = 60, NeededStrength = 70, NeededTechnique = 75 },
            new Position { Id = 2, Name = "Defender", NeededSpeed = 65, NeededEndurance = 75, NeededStrength = 80, NeededTechnique = 65 },
            new Position { Id = 3, Name = "Midfielder", NeededSpeed = 75, NeededEndurance = 85, NeededStrength = 65, NeededTechnique = 80 },
            new Position { Id = 4, Name = "Forward", NeededSpeed = 85, NeededEndurance = 75, NeededStrength = 70, NeededTechnique = 85 }
        );
    }
}

