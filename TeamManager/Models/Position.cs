using System.ComponentModel.DataAnnotations;

namespace TeamManager.Models;

public class Position
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int NeededSpeed { get; set; }

    [Range(1, 100)]
    public int NeededEndurance { get; set; }

    [Range(1, 100)]
    public int NeededStrength { get; set; }

    [Range(1, 100)]
    public int NeededTechnique { get; set; }

    public List<Player> Players { get; set; } = new();
}

