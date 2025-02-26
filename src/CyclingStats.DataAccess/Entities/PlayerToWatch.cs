using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey( "UserId", "WcsUserName")]
public class PlayerToWatch
{
    public string UserId { get; set; }
    public string? PlayerUserId { get; set; }
    public string? Name { get; set; }
    public string WcsUserName { get; set; }
    public bool MainOpponent { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public virtual User? Player { get; set; }
}