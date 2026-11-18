using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using TeamManager.Services;

namespace TeamManager.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class TeamsController(TeamService teamService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var teams = await teamService.GetAllAsync();
        return View(teams);
    }
        public async Task<IActionResult> Details(int id)
    {
        var team = await teamService.GetByIdWithPlayersAsync(id);
        if (team is null)
        {
            return NotFound();
        }

        return View(team);
    }
}