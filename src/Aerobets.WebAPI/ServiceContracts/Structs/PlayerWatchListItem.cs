using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class PlayerWatchListItem
{
    public string? PlayerUserId { get; set; }
    public string? Name { get; set; }
    public string WcsUserName { get; set; }
    public bool MainOpponent { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public UserSummary? User { get; set; }
    
    public static PlayerWatchListItem FromDomain(PlayerWatcher watchItem)
    {
        var playerUser = watchItem.Player;
        var userSummary = playerUser == null ? null : UserSummary.FromDomain(playerUser);
        return new PlayerWatchListItem
        {
            PlayerUserId = watchItem.PlayerUserId, Name = watchItem.Name, WcsUserName = watchItem.WcsUserName,
            MainOpponent = watchItem.MainOpponent, Comment = watchItem.Comment, Created = watchItem.Created,
            User = userSummary
        };
    }
}