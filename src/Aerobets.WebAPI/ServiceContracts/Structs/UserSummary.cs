using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class UserSummary
{
    public string Name { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string? WcsUserName { get; set; }

    public static UserSummary FromDomain(UserDetails userDetails)
    {
        return new UserSummary
        {
            UserId = userDetails.UserId,
            Name = userDetails.Name,
            Email = userDetails.Email,
            WcsUserName = userDetails.WcsUserName,
        };
    }
}