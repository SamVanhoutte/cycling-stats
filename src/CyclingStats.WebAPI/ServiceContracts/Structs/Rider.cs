using CyclingStats.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Structs;

public class Rider
{
    public string? Id { get; set; }
    public string? PcsId { get; set; }
    public string? Name { get; set; }
    public string? Team { get; set; }
    public int Weight{get;set;}
    public int Height{get;set;}

    public int GC{get;set;}
    public int Sprinter{get;set;}
    public int Puncheur{get;set;}
    public int OneDay{get;set;}
    public int Climber{get;set;}
    public int TimeTrialist{get;set;}
    public int UciRanking{get;set;}
    public int PcsRanking{get;set;}
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
    public string PcsRiderId { get; set; }
    public string? Status { get; set; }
    public string Type { get; set; }

    public string WcsUrl => $"https://www.worldcyclingstats.com/en/rider/{Id}/{DateTime.UtcNow.Year}";
    public string PcsUrl => $"https://www.procyclingstats.com/rider/{Id}/{DateTime.UtcNow.Year}";
    public static Rider FromDomain(Models.Rider rider)
    {
        return new Rider
        {
            PcsId = rider.PcsId,
            Name = rider.Name,
            Team = rider.Team,
            Weight = rider.Weight,
            Height = rider.Height,
            GC = rider.GC,
            Sprinter = rider.Sprinter,
            Puncheur = rider.Puncheur,
            OneDay = rider.OneDay,
            Climber = rider.Climber,
            TimeTrialist = rider.TimeTrialist,
            UciRanking = rider.UciRanking,
            PcsRanking = rider.PcsRanking,
            BirthYear = rider.BirthYear,
            Ranking2019 = rider.Ranking2019,
            Ranking2020 = rider.Ranking2020,
            Ranking2021 = rider.Ranking2021,
            Ranking2022 = rider.Ranking2022,
            Ranking2023 = rider.Ranking2023,
            Ranking2024 = rider.Ranking2024,
            Ranking2025 = rider.Ranking2025,
            Ranking2026 = rider.Ranking2026,
            DetailsCompleted = rider.DetailsCompleted,
            Status = rider.Status.ToString(),
            Type = rider.RiderType.ToString()
        };
    }

}