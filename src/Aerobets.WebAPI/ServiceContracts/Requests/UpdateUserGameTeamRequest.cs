using CyclingStats.Models;
using CyclingStats.WebApi.Models;

namespace Aerobets.WebAPI.ServiceContracts.Requests;

public class UpdateUserGameTeamRequest
{
    public string Name { get; set; }
    public string Comment { get; set; }

    public TeamRider[] Riders { get; set; }

}