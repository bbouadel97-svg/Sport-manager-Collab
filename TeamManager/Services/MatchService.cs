using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class MatchService(SportManagerDbContext dbContext)
{
    public async Task<List<Match>> GetAllAsync()
    {
        return await dbContext.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .OrderByDescending(m => m.MatchDate)
            .ToListAsync();
    }

    public async Task AddAsync(Match match)
    {
        if (match.HomeTeamId == match.AwayTeamId)
        {
            throw new InvalidOperationException("A match cannot have the same team as home and away.");
        }

        if (!await dbContext.Teams.AnyAsync(t => t.Id == match.HomeTeamId) ||
            !await dbContext.Teams.AnyAsync(t => t.Id == match.AwayTeamId))
        {
            throw new InvalidOperationException("One or both selected teams are missing.");
        }

        try
        {
            dbContext.Matches.Add(match);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not save match. Please try again.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var match = await dbContext.Matches.FindAsync(id);
        if (match is null)
        {
            throw new InvalidOperationException("Match not found.");
        }

        try
        {
            dbContext.Matches.Remove(match);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not delete match. Please try again.");
        }
    }
}

