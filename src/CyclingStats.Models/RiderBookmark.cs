namespace CyclingStats.Models;

public class RiderBookmark
{
    public Rider Rider { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedOn { get; set; }
}