using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class User
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WcsUserName { get; set; }
    public string? Language { get; set; }
    public string? AuthenticationProviderId { get; set; }

    public static User FromDomain(UserDetails userDetails)
    {
        return new User
        {
            UserId = userDetails.UserId,
            Email = userDetails.Email,
            PhoneNumber = userDetails.Phone,
            WcsUserName = userDetails.WcsUserName,
            Language = userDetails.Language,
            AuthenticationProviderId = userDetails.AuthenticationId
        };
    }

}