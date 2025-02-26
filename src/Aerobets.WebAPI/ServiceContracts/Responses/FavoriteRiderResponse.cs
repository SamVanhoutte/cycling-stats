using Aerobets.WebAPI.ServiceContracts.Structs;

namespace Aerobets.WebAPI.ServiceContracts.Responses;

public class FavoriteRiderResponse(FavoriteRider[] favorites)
{
    public long Count { get; set; } = favorites.Length;
    public FavoriteRider[] Favorites { get; set; } = favorites;
}