using Aerobets.WebAPI.ServiceContracts.Structs;

namespace Aerobets.WebAPI.ServiceContracts.Responses;

public class UserGamesResponse(UserGame[] games)
{
    public long Count { get; set; } = games.Length;
    public UserGame[] Games { get; set; } = games;
}