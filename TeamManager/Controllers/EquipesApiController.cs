using Microsoft.AspNetCore.Mvc;
using TeamManager.DTOs;
using TeamManager.Services;

namespace TeamManager.Controllers;

[ApiController]
[Route("api")]
public class EquipesApiController(TeamService teamService) : ControllerBase
{
    [HttpGet("equipes")]
    public async Task<ActionResult<List<TeamDto>>> GetAll()
    {
        var teams = await teamService.GetAllAsync();
        var result = teams.Select(ApiDtoMapper.ToTeamDto).ToList();

        return Ok(result);
    }

    [HttpGet("equipe/{id:int}")]
    public async Task<ActionResult<TeamDetailsDto>> GetById(int id)
    {
        var team = await teamService.GetByIdWithPlayersAsync(id);

        if (team is null)
        {
            return NotFound(new { message = "Équipe introuvable." });
        }

        return Ok(ApiDtoMapper.ToTeamDetailsDto(team));
    }
}