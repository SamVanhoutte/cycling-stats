using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class WcsUserSettings
{
    public string? UserName { get; set; }

    public static WcsUserSettings FromDomain(UserDetails userDetails)
    {
        return new WcsUserSettings
        {
            UserName = userDetails.WcsUserName,
        };
    }

}