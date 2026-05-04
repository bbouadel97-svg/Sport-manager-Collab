using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class PlayerService(SportManagerDbContext dbContext)
{
    public async Task<List<Player>> GetAllAsync()
    {
        return await dbContext.Players
            .AsNoTracking()
            .Include(p => p.Team)
            .Include(p => p.Position)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(int id)
    {
        return await dbContext.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Player player)
    {
        await ValidateTeamAndPositionAsync(player.TeamId, player.PositionId);

        try
        {
            dbContext.Players.Add(player);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not save player. Please check data and try again.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var player = await dbContext.Players.FindAsync(id);
        if (player is null)
        {
            throw new InvalidOperationException("Player not found.");
        }

        try
        {
            dbContext.Players.Remove(player);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not delete player. Please try again.");
        }
    }

    public async Task UpdateAsync(Player updatedPlayer)
    {
        var player = await dbContext.Players.FindAsync(updatedPlayer.Id);
        if (player is null)
        {
            throw new InvalidOperationException("Player not found.");
        }

        await ValidateTeamAndPositionAsync(updatedPlayer.TeamId, updatedPlayer.PositionId);

        player.FirstName = updatedPlayer.FirstName;
        player.LastName = updatedPlayer.LastName;
        player.FullName = $"{updatedPlayer.FirstName} {updatedPlayer.LastName}";
        player.Age = updatedPlayer.Age;
        player.Number = updatedPlayer.Number;
        player.TeamId = updatedPlayer.TeamId;
        player.PositionId = updatedPlayer.PositionId;
        player.Speed = updatedPlayer.Speed;
        player.Endurance = updatedPlayer.Endurance;
        player.Strength = updatedPlayer.Strength;
        player.Technique = updatedPlayer.Technique;
        player.IsInjured = updatedPlayer.IsInjured;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not update player. Please try again.");
        }
    }

    private async Task ValidateTeamAndPositionAsync(int teamId, int positionId)
    {
        if (teamId <= 0)
        {
            throw new InvalidOperationException("Missing team. Please select a team.");
        }

        if (positionId <= 0)
        {
            throw new InvalidOperationException("Missing position. Please select a position.");
        }

        if (!await dbContext.Teams.AnyAsync(t => t.Id == teamId))
        {
            throw new InvalidOperationException("Selected team no longer exists.");
        }

        if (!await dbContext.Positions.AnyAsync(p => p.Id == positionId))
        {
            throw new InvalidOperationException("Selected position no longer exists.");
        }
    }
}

