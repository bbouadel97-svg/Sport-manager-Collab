using System.ComponentModel.DataAnnotations;

namespace TeamManager.Models;

public class Match
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Home team is required.")]
    public int HomeTeamId { get; set; }

    public Team? HomeTeam { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Away team is required.")]
    public int AwayTeamId { get; set; }

    public Team? AwayTeam { get; set; }

    [Required]
    public DateTime MatchDate { get; set; } = DateTime.Today;

    [Range(0, 99)]
    public int? HomeScore { get; set; }

    [Range(0, 99)]
    public int? AwayScore { get; set; }

    public bool IsPlayed { get; set; }
}

