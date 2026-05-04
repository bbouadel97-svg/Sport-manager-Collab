using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class InjuryService(SportManagerDbContext dbContext)
{
    private readonly Random _random = new();

    public async Task<List<Injury>> GetAllAsync()
    {
        return await dbContext.Injuries
            .AsNoTracking()
            .Include(i => i.Player)
            .ThenInclude(p => p!.Team)
            .OrderByDescending(i => i.InjuryDate)
            .ToListAsync();
    }

    public async Task<List<Injury>> GetActiveInjuriesAsync()
    {
        return await dbContext.Injuries
            .AsNoTracking()
            .Include(i => i.Player)
            .ThenInclude(p => p!.Team)
            .Where(i => !i.IsRecovered)
            .OrderByDescending(i => i.InjuryDate)
            .ToListAsync();
    }

    public async Task AddAsync(Injury injury)
    {
        dbContext.Injuries.Add(injury);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var injury = await dbContext.Injuries.FindAsync(id);
        if (injury is null)
        {
            return;
        }

        dbContext.Injuries.Remove(injury);
        await dbContext.SaveChangesAsync();
    }

    public async Task MarkRecoveredAsync(int injuryId)
    {
        var injury = await dbContext.Injuries
            .Include(i => i.Player)
            .FirstOrDefaultAsync(i => i.Id == injuryId);

        if (injury is null)
        {
            throw new InvalidOperationException("Injury record not found.");
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

    public void CreateMatchInjuries(IEnumerable<Player> playersWhoPlayed)
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

