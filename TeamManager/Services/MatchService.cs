using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class MatchService(SportManagerDbContext dbContext, CurrentUserService currentUser)
{
    public async Task<List<Match>> GetAllAsync()
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Matches
            .AsNoTracking()
            .Where(match => match.UserId == null || match.UserId == userId)
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .OrderByDescending(m => m.MatchDate)
            .ToListAsync();
    }

    public async Task AddAsync(Match match)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        if (match.HomeTeamId == match.AwayTeamId)
        {
            throw new InvalidOperationException("A match cannot have the same team as home and away.");
        }

        if (!await dbContext.Teams.AnyAsync(team => team.Id == match.HomeTeamId && team.UserId == userId) ||
            !await dbContext.Teams.AnyAsync(team => team.Id == match.AwayTeamId && team.UserId == userId))
        {
            throw new InvalidOperationException("Both teams must belong to the current user.");
        }

        try
        {
            match.UserId = userId;
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
        var userId = await currentUser.GetRequiredUserIdAsync();
        var match = await dbContext.Matches.FirstOrDefaultAsync(candidate =>
            candidate.Id == id && candidate.UserId == userId);
        if (match is null)
        {
            throw new InvalidOperationException("Match not found or cannot be modified.");
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

