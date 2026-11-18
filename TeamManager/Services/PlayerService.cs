using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class PlayerService(SportManagerDbContext dbContext, CurrentUserService currentUser)
{
    public async Task<List<Player>> GetAllAsync()
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Players
            .AsNoTracking()
            .Where(player => player.UserId == null || player.UserId == userId)
            .Include(p => p.Team)
            .Include(p => p.Position)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(int id)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Players
            .AsNoTracking()
            .Where(player => player.Id == id && (player.UserId == null || player.UserId == userId))
            .Include(player => player.Team)
            .Include(player => player.Position)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Player player)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        await ValidateTeamAndPositionAsync(player.TeamId, player.PositionId, userId);
        player.UserId = userId;

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
        var userId = await currentUser.GetRequiredUserIdAsync();
        var player = await dbContext.Players.FirstOrDefaultAsync(candidate =>
            candidate.Id == id && candidate.UserId == userId);
        if (player is null)
        {
            throw new InvalidOperationException("Player not found or cannot be modified.");
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
        var userId = await currentUser.GetRequiredUserIdAsync();
        var player = await dbContext.Players.FirstOrDefaultAsync(candidate =>
            candidate.Id == updatedPlayer.Id && candidate.UserId == userId);
        if (player is null)
        {
            throw new InvalidOperationException("Player not found or cannot be modified.");
        }

        await ValidateTeamAndPositionAsync(updatedPlayer.TeamId, updatedPlayer.PositionId, userId);

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

    private async Task ValidateTeamAndPositionAsync(int teamId, int positionId, int userId)
    {
        if (teamId <= 0)
        {
            throw new InvalidOperationException("Missing team. Please select a team.");
        }

        if (positionId <= 0)
        {
            throw new InvalidOperationException("Missing position. Please select a position.");
        }

        if (!await dbContext.Teams.AnyAsync(team => team.Id == teamId && team.UserId == userId))
        {
            throw new InvalidOperationException("Selected team is not owned by the current user.");
        }

        if (!await dbContext.Positions.AnyAsync(p => p.Id == positionId))
        {
            throw new InvalidOperationException("Selected position no longer exists.");
        }
    }
}

