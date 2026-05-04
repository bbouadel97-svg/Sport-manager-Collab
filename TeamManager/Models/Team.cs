using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamManager.Models;

public class Team
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string CoachName { get; set; } = string.Empty;

    [NotMapped]
    public int PlayerCount { get; set; }

    public List<Player> Players { get; set; } = new();

    public List<Match> HomeMatches { get; set; } = new();

    public List<Match> AwayMatches { get; set; } = new();
}

