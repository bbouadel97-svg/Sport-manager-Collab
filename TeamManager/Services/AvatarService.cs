using TeamManager.Models;

namespace TeamManager.Services;

public class AvatarService
{
    public string GetAvatarUrl(Player player)
    {
        var seed = BuildSeed(player);
        return $"https://api.dicebear.com/9.x/bottts/svg?seed={Uri.EscapeDataString(seed)}";
    }

    private static string BuildSeed(Player player)
    {
        if (player.Id > 0)
        {
            return $"player-{player.Id}";
        }

        var first = player.FirstName?.Trim().ToLowerInvariant() ?? string.Empty;
        var last = player.LastName?.Trim().ToLowerInvariant() ?? string.Empty;

        var nameSeed = $"{first}-{last}".Trim('-');
        return string.IsNullOrWhiteSpace(nameSeed) ? "player-new" : nameSeed;
    }
}

