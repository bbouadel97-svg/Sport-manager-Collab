using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class InjuryService(SportManagerDbContext dbContext, CurrentUserService currentUser)
{
    private readonly Random _random = new();

    public async Task<List<Injury>> GetAllAsync()
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Injuries
            .AsNoTracking()
            .Where(injury => injury.UserId == null || injury.UserId == userId)
            .Include(i => i.Player)
            .ThenInclude(p => p!.Team)
            .OrderByDescending(i => i.InjuryDate)
            .ToListAsync();
    }

    public async Task<List<Injury>> GetActiveInjuriesAsync()
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        return await dbContext.Injuries
            .AsNoTracking()
            .Where(injury => (injury.UserId == null || injury.UserId == userId) && !injury.IsRecovered)
            .Include(i => i.Player)
            .ThenInclude(p => p!.Team)
            .OrderByDescending(i => i.InjuryDate)
            .ToListAsync();
    }

    public async Task AddAsync(Injury injury)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        if (!await dbContext.Players.AnyAsync(player => player.Id == injury.PlayerId && player.UserId == userId))
        {
            throw new InvalidOperationException("The player is not owned by the current user.");
        }

        injury.UserId = userId;
        dbContext.Injuries.Add(injury);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        var injury = await dbContext.Injuries.FirstOrDefaultAsync(candidate =>
            candidate.Id == id && candidate.UserId == userId);
        if (injury is null)
        {
            return;
        }

        dbContext.Injuries.Remove(injury);
        await dbContext.SaveChangesAsync();
    }

    public async Task MarkRecoveredAsync(int injuryId)
    {
        var userId = await currentUser.GetRequiredUserIdAsync();
        var injury = await dbContext.Injuries
            .Include(i => i.Player)
            .FirstOrDefaultAsync(i => i.Id == injuryId && i.UserId == userId);

        if (injury is null)
        {
            throw new InvalidOperationException("Injury record not found or cannot be modified.");
        }

        injury.IsRecovered = true;

        if (injury.Player is not null)
        {
            injury.Player.IsInjured = false;
        }

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not update injury status. Please try again.");
        }
    }

    public void CreateMatchInjuries(IEnumerable<Player> playersWhoPlayed, int userId)
    {
        foreach (var player in playersWhoPlayed)
        {
            if (player.IsInjured)
            {
                continue;
            }

            var injuryChance = player.Endurance < 40 ? 0.10 : 0.05;
            if (_random.NextDouble() > injuryChance)
            {
                continue;
            }

            player.IsInjured = true;

            dbContext.Injuries.Add(new Injury
            {
                UserId = userId,
                PlayerId = player.Id,
                Description = BuildInjuryDescription(player),
                InjuryDate = DateTime.Today,
                RecoveryDays = _random.Next(3, 15),
                IsRecovered = false
            });
        }
    }

    private string BuildInjuryDescription(Player player)
    {
        var options = new[]
        {
            "Muscle strain after a fast sprint.",
            "Ankle twist during a challenge.",
            "Knee pain after landing awkwardly.",
            "Minor hamstring pull in the second half."
        };

        var detail = options[_random.Next(options.Length)];
        return $"{player.FirstName} {player.LastName}: {detail}";
    }
}

