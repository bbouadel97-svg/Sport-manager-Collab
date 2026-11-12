using Microsoft.AspNetCore.Mvc;
using TeamManager.DTOs;
using TeamManager.Services;

namespace TeamManager.Controllers;

[ApiController]
[Route("api")]
public class JoueursApiController(PlayerService playerService) : ControllerBase
{
    [HttpGet("joueurs")]
    public async Task<ActionResult<List<PlayerDto>>> GetAll()
    {
        var players = await playerService.GetAllAsync();
        var result = players.Select(player => ApiDtoMapper.ToPlayerDto(player)).ToList();

        return Ok(result);
    }

    [HttpGet("joueur/{id:int}")]
    public async Task<ActionResult<PlayerDto>> GetById(int id)
    {
        var player = await playerService.GetByIdAsync(id);

        if (player is null)
        {
            return NotFound(new { message = "Joueur introuvable." });
        }

        return Ok(ApiDtoMapper.ToPlayerDto(player));
    }
}