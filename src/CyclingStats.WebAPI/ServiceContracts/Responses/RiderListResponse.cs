using CyclingStats.WebApi.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Responses;

public class RiderListResponse(Rider[] riderDetails)
{
    public long Count { get; set; } = riderDetails.Length;
    public Rider[] Riders { get; set; } = riderDetails;
}