using CyclingStats.Models;

namespace Aerobets.WebAPI.ServiceContracts.Structs;

public class RiderBookmarkSummary
{
    public string RiderId { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedOn { get; set; }

    public static RiderBookmarkSummary? FromDomain(RiderBookmark? bookmark)
    {
        return bookmark==null ? null: new RiderBookmarkSummary
        {
            RiderId = bookmark.Rider.Id, Comment = bookmark.Comment, CreatedOn = bookmark.CreatedOn
        };
    }
}