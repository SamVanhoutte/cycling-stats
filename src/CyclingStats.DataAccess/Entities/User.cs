using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("UserId")]
public class User
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string? AuthenticationId { get; set; }
    public string? WcsUserName { get; set; }
    public string? WcsPasswordEncrypted { get; set; }
    public string? Phone { get; set; }
    public string? Language { get; set; }
    public DateTime Updated { get; set; }
    
}