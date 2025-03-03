using CyclingStats.WebApi.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class UserGameTeam
{
    public string TeamId { get; set; }
    public string Name { get; set; }
    public string Comment { get; set; }
    public DateTime LastSaved { get; set; }
    public TeamRider[] Riders { get; set; }

    public static UserGameTeam FromDomain(CyclingStats.Models.UserGameTeam team)
    {
        var riders = new List<Aerobets.WebAPI.ServiceContracts.Structs.UserRiderRaceInfo>{};
        var userTeam = new UserGameTeam
        {
            TeamId = team.TeamId, Comment = team.Comment, Name = team.Name, LastSaved = team.LastSaved,
            Riders = team.Riders.Select(r => new TeamRider
            {
                RiderId = r.RiderId, RiderType = r.RiderType,
                Stars = r.Stars, Youth = r.Youth
            }).ToArray()
        };
        return userTeam;
    }
}