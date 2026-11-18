using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class TeamService(SportManagerDbContext dbContext, CurrentUserService currentUser)
{
    public async Task<List<Team>> GetAllAsync()
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Teams
            .AsNoTracking()
            .Where(team => team.UserId == null || team.UserId == userId)
            .Select(t => new Team
            {
                Id = t.Id,
                UserId = t.UserId,
                Name = t.Name,
                City = t.City,
                CoachName = t.CoachName,
                PlayerCount = t.Players.Count(player => player.UserId == null || player.UserId == userId)
            })
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Team>> GetOwnedAsync()
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Teams
            .AsNoTracking()
            .Where(team => team.UserId == userId)
            .OrderBy(team => team.Name)
            .ToListAsync();
    }

    public async Task<Team?> GetOwnedByIdAsync(int id)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Teams.FirstOrDefaultAsync(team =>
            team.Id == id && team.UserId == userId);
    }

    public async Task AddAsync(Team team)
    {
        try
        {
            team.UserId = await currentUser.GetRequiredUserIdAsync();
            dbContext.Teams.Add(team);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not save team. Please try again.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        var team = await dbContext.Teams.FirstOrDefaultAsync(candidate =>
            candidate.Id == id && candidate.UserId == userId);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found or cannot be modified.");
        }

        try
        {
            dbContext.Teams.Remove(team);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not delete team. It may be linked to other records.");
        }
    }

    public async Task UpdateAsync(Team updatedTeam)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        var team = await dbContext.Teams.FirstOrDefaultAsync(candidate =>
            candidate.Id == updatedTeam.Id && candidate.UserId == userId);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found or cannot be modified.");
        }

        team.Name = updatedTeam.Name;
        team.City = updatedTeam.City;
        team.CoachName = updatedTeam.CoachName;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not update team. Please try again.");
        }
    }
        public async Task<Team?> GetByIdWithPlayersAsync(int id)
    {
            var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Teams
            .AsNoTracking()
                .Where(team => team.Id == id && (team.UserId == null || team.UserId == userId))
                .Include(t => t.Players.Where(player => player.UserId == null || player.UserId == userId))
                .ThenInclude(p => p.Position)
                .FirstOrDefaultAsync();
    }
}

