using System.ComponentModel.DataAnnotations;

namespace TeamManager.Models;

public class Injury
{
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    public Player? Player { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime InjuryDate { get; set; } = DateTime.Today;

    [Range(1, 365)]
    public int RecoveryDays { get; set; } = 7;

    public bool IsRecovered { get; set; }
}

