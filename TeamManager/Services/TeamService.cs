using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class TeamService(SportManagerDbContext dbContext)
{
    public async Task<List<Team>> GetAllAsync()
    {
        return await dbContext.Teams
            .AsNoTracking()
            .Select(t => new Team
            {
                Id = t.Id,
                Name = t.Name,
                City = t.City,
                CoachName = t.CoachName,
                PlayerCount = t.Players.Count
            })
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Team team)
    {
        try
        {
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
        var team = await dbContext.Teams.FindAsync(id);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found.");
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
        var team = await dbContext.Teams.FindAsync(updatedTeam.Id);
        if (team is null)
        {
            throw new InvalidOperationException("Team not found.");
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
}

