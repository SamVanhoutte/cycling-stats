using CyclingStats.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Structs;

public class RiderSummary
{
    public string RiderId { get; set; }
    public string TeamId { get; set; }
    public string? Name { get; set; }
    
    public string? PcsId { get; set; }

    public int BirthYear{get;set;}
    public int Ranking2019{get;set;}
    public int Ranking2020{get;set;}
    public int Ranking2021{get;set;}
    public int Ranking2022{get;set;}
    public int Ranking2023{get;set;}
    public int Ranking2024{get;set;}
    public int Ranking2025{get;set;}
    public int Ranking2026{get;set;}
    public bool DetailsCompleted { get; set; }
    public RiderStatus? Status { get; set; }
}