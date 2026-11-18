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
    public DbSet<User> Users => Set<User>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique().HasDatabaseName("ix_users_email");
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.ToTable("revoked_tokens");
            entity.HasKey(token => token.JwtId);
            entity.Property(token => token.JwtId).HasColumnName("jwt_id").HasMaxLength(36);
            entity.Property(token => token.ExpiresAt).HasColumnName("expires_at");
            entity.HasIndex(token => token.ExpiresAt).HasDatabaseName("ix_revoked_tokens_expires_at");
        });

        modelBuilder.Entity<Team>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(team => team.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Player>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(player => player.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(match => match.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Injury>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(injury => injury.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>().HasIndex(team => team.UserId);
        modelBuilder.Entity<Player>().HasIndex(player => player.UserId);
        modelBuilder.Entity<Match>().HasIndex(match => match.UserId);
        modelBuilder.Entity<Injury>().HasIndex(injury => injury.UserId);

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

