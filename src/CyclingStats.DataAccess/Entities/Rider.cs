using System.ComponentModel.DataAnnotations.Schema;
using CyclingStats.Models;
using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;


[PrimaryKey("Id")]
public class Rider
{
    public string Id { get; set; }
    public string? PcsId { get; set; }
    public string Name { get; set; }
    public string Team { get; set; }
    public int? RiderType {get;set;}
    public int BirthYear{get;set;}
    public int Weight{get;set;}
    public int Height{get;set;}
    public int Ranking2019{get;set;}
    public int Ranking2020{get;set;}
    public int Ranking2021{get;set;}
    public int Ranking2022{get;set;}
    public int Ranking2023{get;set;}
    public int Ranking2024{get;set;}
    public int Ranking2025{get;set;}
    public int Ranking2026{get;set;}
    public virtual ICollection<RiderProfile>? Profiles { get; set; }
    public bool DetailsCompleted { get; set; }
    public DateTime Updated { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public RiderStatus Status { get; set; }
    
    public virtual ICollection<User> FavoritedBy { get; set; } = new List<User>();
    public virtual ICollection<UserRider> UserRiders { get; set; } = new List<UserRider>();
}