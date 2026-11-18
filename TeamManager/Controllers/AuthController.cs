using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.DTOs;
using TeamManager.Models;
using TeamManager.Services;

namespace TeamManager.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    SportManagerDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            return BadRequest(new { message = "Le nom est obligatoire." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return Conflict(new { message = "Cette adresse e-mail est déjà utilisée." });
        }

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = string.Empty
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            user.Id,
            user.Name,
            user.Email
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);
        if (user is null)
        {
            return Unauthorized(new { message = "Adresse e-mail ou mot de passe invalide." });
        }

        var verificationResult =
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Adresse e-mail ou mot de passe invalide." });
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await dbContext.SaveChangesAsync();
        }

        var token = tokenService.CreateToken(user, out _, out var expiresAtUtc);
        return Ok(new AuthResponse(token, expiresAtUtc, user.Id, user.Name, user.Email));
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
        var jwtId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expiration = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (string.IsNullOrWhiteSpace(jwtId) || !long.TryParse(expiration, out var expirationSeconds))
        {
            return Unauthorized();
        }

        if (!await dbContext.RevokedTokens.AnyAsync(token => token.JwtId == jwtId))
        {
            dbContext.RevokedTokens.Add(new RevokedToken
            {
                JwtId = jwtId,
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expirationSeconds).UtcDateTime
            });
            await dbContext.SaveChangesAsync();
        }

        return NoContent();
    }
}
