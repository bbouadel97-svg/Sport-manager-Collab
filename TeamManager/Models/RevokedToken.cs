namespace TeamManager.Models;

public class RevokedToken
{
    public required string JwtId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
