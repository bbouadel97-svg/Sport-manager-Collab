using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamManager.Models;

public class Player
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Range(15, 45)]
    public int Age { get; set; }

    [Required]
    [StringLength(100)]
    // Kept for compatibility with existing simple pages.
    public string FullName { get; set; } = string.Empty;

    [Range(0, 99)]
    public int Number { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Team is required.")]
    public int TeamId { get; set; }

    public Team? Team { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Position is required.")]
    public int PositionId { get; set; }

    public Position? Position { get; set; }

    [Range(1, 100)]
    public int Speed { get; set; }

    [Range(1, 100)]
    public int Endurance { get; set; }

    [Range(1, 100)]
    public int Strength { get; set; }

    [Range(1, 100)]
    public int Technique { get; set; }

    [NotMapped]
    public decimal OverallRating => Math.Round((Speed + Endurance + Strength + Technique) / 4m, 1);

    public bool IsInjured { get; set; }

    public List<Injury> Injuries { get; set; } = new();
}

