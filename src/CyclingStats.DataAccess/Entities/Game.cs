using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("RaceId")]
public class Game
{
    public string RaceId { get; set; }
    public int? StarBudget { get; set; }
    public int Status { get; set; }
    public DateTime Updated { get; set; }
    
    public virtual ICollection<StartGridEntry>? StartList { get; set; }
}