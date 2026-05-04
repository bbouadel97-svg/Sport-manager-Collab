using TeamManager.Models;

namespace TeamManager.Data;

public static class SportManagerSeeder
{
    public static void Seed(SportManagerDbContext dbContext)
    {
        SeedTeams(dbContext);
        SeedPositions(dbContext);
        SeedPlayers(dbContext);
        SeedMatches(dbContext);
        SeedInjuries(dbContext);
    }

    private static void SeedTeams(SportManagerDbContext dbContext)
    {
        if (dbContext.Teams.Any())
        {
            return;
        }

        dbContext.Teams.AddRange(
            new Team { Name = "Springfield Lions", City = "Springfield", CoachName = "Mark Jensen" },
            new Team { Name = "Riverside Sharks", City = "Riverside", CoachName = "Paulo Costa" },
            new Team { Name = "Hillview Eagles", City = "Hillview", CoachName = "Ivan Novak" }
        );

        dbContext.SaveChanges();
    }

    private static void SeedPositions(SportManagerDbContext dbContext)
    {
        var positionTemplates = new List<Position>
        {
            new() { Name = "Goalkeeper", NeededSpeed = 55, NeededEndurance = 60, NeededStrength = 70, NeededTechnique = 75 },
            new() { Name = "Defender", NeededSpeed = 65, NeededEndurance = 75, NeededStrength = 80, NeededTechnique = 65 },
            new() { Name = "Midfielder", NeededSpeed = 75, NeededEndurance = 85, NeededStrength = 65, NeededTechnique = 80 },
            new() { Name = "Winger", NeededSpeed = 85, NeededEndurance = 78, NeededStrength = 55, NeededTechnique = 82 },
            new() { Name = "Forward", NeededSpeed = 83, NeededEndurance = 72, NeededStrength = 68, NeededTechnique = 86 }
        };

        foreach (var template in positionTemplates)
        {
            if (dbContext.Positions.Any(p => p.Name == template.Name))
            {
                continue;
            }

            dbContext.Positions.Add(template);
        }

        dbContext.SaveChanges();
    }

    private static void SeedPlayers(SportManagerDbContext dbContext)
    {
        if (dbContext.Players.Any())
        {
            return;
        }

        var teams = dbContext.Teams.ToDictionary(t => t.Name, t => t.Id);
        var positions = dbContext.Positions.ToDictionary(p => p.Name, p => p.Id);

        var players = new List<Player>
        {
            CreatePlayer("Liam", "Carter", 21, teams["Springfield Lions"], positions["Goalkeeper"], 10, 52, 65, 70, 74, false),
            CreatePlayer("Ethan", "Brooks", 24, teams["Springfield Lions"], positions["Defender"], 4, 58, 72, 78, 60, false),
            CreatePlayer("Mason", "Cole", 20, teams["Springfield Lions"], positions["Midfielder"], 8, 76, 81, 63, 79, false),
            CreatePlayer("Noah", "Price", 19, teams["Springfield Lions"], positions["Winger"], 11, 88, 74, 49, 80, false),
            CreatePlayer("Lucas", "Reed", 23, teams["Springfield Lions"], positions["Forward"], 9, 84, 69, 66, 85, false),
            CreatePlayer("Owen", "Clark", 25, teams["Springfield Lions"], positions["Defender"], 5, 43, 60, 71, 48, false),

            CreatePlayer("Aiden", "Morris", 22, teams["Riverside Sharks"], positions["Goalkeeper"], 1, 50, 63, 69, 73, false),
            CreatePlayer("Leo", "Bauer", 26, teams["Riverside Sharks"], positions["Defender"], 3, 62, 74, 82, 58, false),
            CreatePlayer("Mateo", "Silva", 21, teams["Riverside Sharks"], positions["Midfielder"], 6, 79, 84, 67, 83, false),
            CreatePlayer("Noah", "Becker", 20, teams["Riverside Sharks"], positions["Winger"], 7, 73, 66, 52, 71, true),
            CreatePlayer("Daniel", "Pereira", 24, teams["Riverside Sharks"], positions["Forward"], 10, 86, 77, 70, 88, false),
            CreatePlayer("Jonas", "Fischer", 19, teams["Riverside Sharks"], positions["Defender"], 2, 40, 57, 64, 46, false),

            CreatePlayer("Oliver", "Meyer", 23, teams["Hillview Eagles"], positions["Goalkeeper"], 12, 56, 67, 72, 76, false),
            CreatePlayer("Henry", "Stone", 22, teams["Hillview Eagles"], positions["Defender"], 4, 66, 76, 81, 62, false),
            CreatePlayer("Jack", "Young", 20, teams["Hillview Eagles"], positions["Midfielder"], 8, 70, 79, 60, 74, false),
            CreatePlayer("Elijah", "Turner", 18, teams["Hillview Eagles"], positions["Winger"], 11, 47, 58, 45, 54, false),
            CreatePlayer("Benjamin", "Gray", 27, teams["Hillview Eagles"], positions["Forward"], 9, 82, 73, 69, 84, false),
            CreatePlayer("Samuel", "Ward", 21, teams["Hillview Eagles"], positions["Defender"], 5, 61, 68, 74, 57, false)
        };

        dbContext.Players.AddRange(players);
        dbContext.SaveChanges();
    }

    private static void SeedMatches(SportManagerDbContext dbContext)
    {
        if (dbContext.Matches.Any())
        {
            return;
        }

        var teams = dbContext.Teams.ToDictionary(t => t.Name, t => t.Id);

        dbContext.Matches.AddRange(
            new Match
            {
                HomeTeamId = teams["Springfield Lions"],
                AwayTeamId = teams["Riverside Sharks"],
                HomeScore = 2,
                AwayScore = 1,
                MatchDate = new DateTime(2026, 4, 20),
                IsPlayed = true
            },
            new Match
            {
                HomeTeamId = teams["Hillview Eagles"],
                AwayTeamId = teams["Springfield Lions"],
                HomeScore = null,
                AwayScore = null,
                MatchDate = new DateTime(2026, 5, 12),
                IsPlayed = false
            }
        );

        dbContext.SaveChanges();
    }

    private static void SeedInjuries(SportManagerDbContext dbContext)
    {
        if (dbContext.Injuries.Any())
        {
            return;
        }

        var injuredPlayer = dbContext.Players.FirstOrDefault(p => p.FirstName == "Noah" && p.LastName == "Becker");
        if (injuredPlayer is null)
        {
            return;
        }

        injuredPlayer.IsInjured = true;

        dbContext.Injuries.Add(new Injury
        {
            PlayerId = injuredPlayer.Id,
            Description = "Hamstring strain during sprint training",
            InjuryDate = new DateTime(2026, 4, 28),
            RecoveryDays = 21,
            IsRecovered = false
        });

        dbContext.SaveChanges();
    }

    private static Player CreatePlayer(
        string firstName,
        string lastName,
        int age,
        int teamId,
        int positionId,
        int number,
        int speed,
        int endurance,
        int strength,
        int technique,
        bool isInjured)
    {
        return new Player
        {
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            Age = age,
            TeamId = teamId,
            PositionId = positionId,
            Number = number,
            Speed = speed,
            Endurance = endurance,
            Strength = strength,
            Technique = technique,
            IsInjured = isInjured
        };
    }
}

