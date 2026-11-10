using Microsoft.AspNetCore.Mvc;
using TeamManager.Services;

namespace TeamManager.Controllers;

public class PlayersController(PlayerService playerService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var players = await playerService.GetAllAsync();
        return View(players);
    }

    public async Task<IActionResult> Details(int id)
    {
        var player = await playerService.GetByIdAsync(id);
        if (player is null)
        {
            return NotFound();
        }

        return View(player);
    }
}
