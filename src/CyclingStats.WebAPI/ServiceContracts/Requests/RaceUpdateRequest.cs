using CyclingStats.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Requests;

public class RaceUpdateRequest
{
    public string RaceId { get; set; }
    public string? PcsId { get; set; }
    public RaceStatus? Status { get; set; }
}