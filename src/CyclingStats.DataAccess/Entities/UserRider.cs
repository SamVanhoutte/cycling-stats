using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey( "UserId", "RiderId")]
public class UserRider
{
    public string RiderId { get; set; }
    public string UserId { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public virtual User User { get; set; }
    public virtual Rider Rider { get; set; }
}