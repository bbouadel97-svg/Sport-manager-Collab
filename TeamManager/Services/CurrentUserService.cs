using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TeamManager.Services;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider)
{
    public async Task<int> GetRequiredUserIdAsync()
    {
        var httpUser = httpContextAccessor.HttpContext?.User;
        if (TryGetUserId(httpUser, out var httpUserId))
        {
            return httpUserId;
        }

        var authenticationStateProvider = serviceProvider.GetService<AuthenticationStateProvider>();
        if (authenticationStateProvider is not null)
        {
            var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (TryGetUserId(authenticationState.User, out var circuitUserId))
            {
                return circuitUserId;
            }
        }

        throw new UnauthorizedAccessException("An authenticated user is required.");
    }

    private static bool TryGetUserId(ClaimsPrincipal? principal, out int userId)
    {
        userId = 0;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var claim = principal.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out userId);
    }
}
