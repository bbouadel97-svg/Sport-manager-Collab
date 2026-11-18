using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TeamManager.DTOs;
using TeamManager.Models;
using TeamManager.Services;

namespace TeamManager.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

    [HttpPost("equipes")]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamRequest request)
    {
        var team = new Team
        {
            Name = request.Name.Trim(),
            City = request.City.Trim(),
            CoachName = request.CoachName.Trim()
        };
        await teamService.AddAsync(team);

        return CreatedAtAction(
            nameof(GetById),
            new { id = team.Id },
            ApiDtoMapper.ToTeamDto(team));
    }

    [HttpPut("equipe/{id:int}")]
    public async Task<IActionResult> Update(int id, CreateTeamRequest request)
    {
        if (await teamService.GetOwnedByIdAsync(id) is null)
        {
            return NotFound(new { message = "Équipe introuvable ou non modifiable." });
        }

        await teamService.UpdateAsync(new Team
        {
            Id = id,
            Name = request.Name.Trim(),
            City = request.City.Trim(),
            CoachName = request.CoachName.Trim()
        });

        return NoContent();
    }

    [HttpDelete("equipe/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await teamService.GetOwnedByIdAsync(id) is null)
        {
            return NotFound(new { message = "Équipe introuvable ou non modifiable." });
        }

        await teamService.DeleteAsync(id);
        return NoContent();
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