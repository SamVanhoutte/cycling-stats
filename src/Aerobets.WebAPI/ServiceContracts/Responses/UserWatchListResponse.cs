using Aerobets.WebAPI.ServiceContracts.Structs;

namespace Aerobets.WebAPI.ServiceContracts.Responses;

public class UserWatchListResponse(PlayerWatchListItem[] watchlist)
{
    public long Count { get; set; } = watchlist.Length;
    public PlayerWatchListItem[] Watchlist { get; set; } = watchlist;

}