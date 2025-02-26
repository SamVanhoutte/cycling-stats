using CyclingStats.DataAccess.Entities;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;
using Rider = CyclingStats.DataAccess.Entities.Rider;

namespace CyclingStats.DataAccess;

public partial class CyclingDbContext 
{
    
    public async Task<ICollection<User>> GetAllUsersAsync()
    {
        var users = await Users.ToListAsync();
        return users;
    }
}