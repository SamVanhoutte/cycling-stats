using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class User
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public string? AuthenticationProviderId { get; set; }
    public WcsUserSettings? WcsSettings { get; set; }

    public static User FromDomain(UserDetails userDetails)
    {
        return new User
        {
            Name = userDetails.Name,
            UserId = userDetails.UserId,
            Email = userDetails.Email,
            PhoneNumber = userDetails.Phone,
            WcsSettings = WcsUserSettings.FromDomain(userDetails),
            Language = userDetails.Language,
            AuthenticationProviderId = userDetails.AuthenticationId
        };
    }

}