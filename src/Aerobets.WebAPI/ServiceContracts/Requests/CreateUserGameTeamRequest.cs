using Aerobets.WebAPI.ServiceContracts.Structs;
using CyclingStats.WebApi.Models;

namespace Aerobets.WebAPI.ServiceContracts.Requests;

public class CreateUserGameTeamRequest
{
    public string Name { get; set; }
    public string Comment { get; set; }

    public TeamRider[] Riders { get; set; }

}

