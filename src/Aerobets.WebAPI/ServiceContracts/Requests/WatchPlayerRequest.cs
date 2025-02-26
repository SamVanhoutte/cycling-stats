namespace Aerobets.WebAPI.ServiceContracts.Requests;

public class WatchPlayerRequest
{
    public string Comment { get; set; }
    public bool MainOpponent { get; set; }
    public string? Name { get; set; }
}