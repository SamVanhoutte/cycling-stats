using Aerobets.WebAPI.ServiceContracts.Structs;

namespace Aerobets.WebAPI.ServiceContracts.Responses;

public class UserGameTeamsResponse(UserGameTeam[] teams)
{
    public long Count { get; set; } = teams.Length;
    public UserGameTeam[] Teams { get; set; } = teams;
}