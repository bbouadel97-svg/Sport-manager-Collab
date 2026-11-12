using TeamManager.Models;

namespace TeamManager.DTOs;

public sealed record TeamDto(
    int Id,
    string Name,
    string City,
    string CoachName,
    int PlayerCount);

public sealed record TeamDetailsDto(
    int Id,
    string Name,
    string City,
    string CoachName,
    List<PlayerDto> Players);

public sealed record PlayerDto(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    int Age,
    int Number,
    int TeamId,
    string? TeamName,
    int PositionId,
    string? PositionName,
    int Speed,
    int Endurance,
    int Strength,
    int Technique,
    decimal OverallRating,
    bool IsInjured);

public static class ApiDtoMapper
{
    public static TeamDto ToTeamDto(Team team) =>
        new(
            team.Id,
            team.Name,
            team.City,
            team.CoachName,
            team.PlayerCount);

    public static TeamDetailsDto ToTeamDetailsDto(Team team) =>
        new(
            team.Id,
            team.Name,
            team.City,
            team.CoachName,
            team.Players
                .OrderBy(player => player.LastName)
                .ThenBy(player => player.FirstName)
                .Select(player => ToPlayerDto(player, team.Name))
                .ToList());

    public static PlayerDto ToPlayerDto(Player player, string? teamNameOverride = null) =>
        new(
            player.Id,
            player.FirstName,
            player.LastName,
            player.FullName,
            player.Age,
            player.Number,
            player.TeamId,
            teamNameOverride ?? player.Team?.Name,
            player.PositionId,
            player.Position?.Name,
            player.Speed,
            player.Endurance,
            player.Strength,
            player.Technique,
            player.OverallRating,
            player.IsInjured);
}