using Microsoft.EntityFrameworkCore;
using TeamManager.Data;

namespace TeamManager.Services;

public class MatchSimulationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? HomeScore { get; init; }
    public int? AwayScore { get; init; }
}

public class MatchSimulationService(SportManagerDbContext dbContext, InjuryService injuryService)
{
    private readonly Random _random = new();

    public async Task<MatchSimulationResult> SimulateMatchAsync(int matchId)
    {
        var match = await dbContext.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match is null)
        {
            return new MatchSimulationResult
            {
                Success = false,
                Message = "Match was not found."
            };
        }

        if (match.HomeTeamId == match.AwayTeamId)
        {
            return new MatchSimulationResult
            {
                Success = false,
                Message = "A team cannot play against itself."
            };
        }

        var homePlayers = await dbContext.Players
            .Where(p => p.TeamId == match.HomeTeamId && !p.IsInjured)
            .ToListAsync();

        var awayPlayers = await dbContext.Players
            .Where(p => p.TeamId == match.AwayTeamId && !p.IsInjured)
            .ToListAsync();

        if (homePlayers.Count < 3 || awayPlayers.Count < 3)
        {
            return new MatchSimulationResult
            {
                Success = false,
                Message = "Both teams must have at least 3 non-injured players."
            };
        }

        var homeStrength = homePlayers.Average(GetPlayerRating);
        var awayStrength = awayPlayers.Average(GetPlayerRating);

        var homeScore = GenerateGoals(homeStrength, awayStrength);
        var awayScore = GenerateGoals(awayStrength, homeStrength);

        match.HomeScore = homeScore;
        match.AwayScore = awayScore;
        match.IsPlayed = true;

        var allPlayersWhoPlayed = homePlayers.Concat(awayPlayers).ToList();
        injuryService.CreateMatchInjuries(allPlayersWhoPlayed);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new MatchSimulationResult
            {
                Success = false,
                Message = "Could not save simulation result. Please try again."
            };
        }

        return new MatchSimulationResult
        {
            Success = true,
            Message = "Match simulation completed.",
            HomeScore = homeScore,
            AwayScore = awayScore
        };
    }

    private static decimal GetPlayerRating(Models.Player player)
    {
        return (player.Speed + player.Endurance + player.Strength + player.Technique) / 4m;
    }

    private int GenerateGoals(decimal teamStrength, decimal opponentStrength)
    {
        var strengthShare = teamStrength / (teamStrength + opponentStrength);
        var goalChance = 0.20m + (0.45m * strengthShare);

        var goals = 0;

        for (var i = 0; i < 6; i++)
        {
            var randomAdjustment = (_random.NextDouble() - 0.5) * 0.12;
            var chance = (double)goalChance + randomAdjustment;

            if (_random.NextDouble() < chance)
            {
                goals++;
            }
        }

        return Math.Clamp(goals, 0, 5);
    }

}

