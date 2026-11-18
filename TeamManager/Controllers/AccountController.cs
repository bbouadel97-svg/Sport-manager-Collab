using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.DTOs;
using TeamManager.Models;

namespace TeamManager.Controllers;

[Route("account")]
public sealed class AccountController(
    SportManagerDbContext dbContext,
    IPasswordHasher<User> passwordHasher) : Controller
{
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginRequest());
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email);
        if (user is null ||
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) ==
            PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou mot de passe invalide.");
            return View(request);
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToLocal(returnUrl);
    }

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterRequest());

    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Name), "Le nom est obligatoire.");
            return View(request);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email))
        {
            ModelState.AddModelError(nameof(request.Email), "Cette adresse e-mail est déjà utilisée.");
            return View(request);
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

        return RedirectToAction(nameof(Login));
    }

    [HttpGet("logout")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : Redirect("/dashboard");
    }
}
