using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class UserGame
{
    public List<UserGameResult> Results { get; set; } = new();
    public string? Comment { get; set; }
    public DateTime Created { get; set; }
    public string Status { get; set; }

    public static UserGame FromDomain(UserGameDetails gameDetails)
    {
        return new UserGame
        {
            Comment = gameDetails.Comment, Results = gameDetails.Results,
            Created = gameDetails.Created, Status = gameDetails.Status.ToString()
        };
    }

}