namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class FavoriteRider
{
    public RiderSummary Rider { get; set; }
    public string Comment { get; set; }
    public DateTime FavoritedOn { get; set; }
}