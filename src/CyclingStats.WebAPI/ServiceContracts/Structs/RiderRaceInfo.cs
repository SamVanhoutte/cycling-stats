using System.Text;
using CyclingStats.Models;

namespace CyclingStats.WebAPI.ServiceContracts.Structs;

public class RiderRaceInfo
{
    public RiderSummary Rider { get; set; }
    public RiderProfile? Profile { get; set; }
    
    public string RiderType { get; set; }
    public int Stars { get; set; }

    public string StarString
    {
        get
        {
            var b = new StringBuilder();
            for (int i = 0; i < Stars; i++)
            {
                b.Append("⭐️");
            }

            return b.ToString();
        }
    }

    public bool YouthRider { get; set; }
    

    public static RiderRaceInfo FromDomain(StartingRider gridEntry)
    {
        var riderRace = gridEntry.Rider;
        return new RiderRaceInfo
        {
            Rider = new RiderSummary
            {
                RiderId = riderRace.Id,
                Name = riderRace.Name,
                TeamId = riderRace.Team,
                PcsId = riderRace.PcsId,
                BirthYear = riderRace.BirthYear,
                Ranking2019 = riderRace.Ranking2019,
                Ranking2020 = riderRace.Ranking2020,
                Ranking2021 = riderRace.Ranking2021,
                Ranking2022 = riderRace.Ranking2022,
                Ranking2023 = riderRace.Ranking2023,
                Ranking2024 = riderRace.Ranking2024,
                Ranking2025 = riderRace.Ranking2025,
                Ranking2026 = riderRace.Ranking2026,
                DetailsCompleted = riderRace.DetailsCompleted,
                Status = riderRace.Status
            },
            Profile = new RiderProfile
            {
                Climber = riderRace.Climber,
                GC = riderRace.GC,
                OneDay = riderRace.OneDay,
                Puncheur = riderRace.Puncheur,
                Sprinter = riderRace.Sprinter,
                TimeTrialist = riderRace.TimeTrialist,
                UciRanking = riderRace.UciRanking,
                PcsRanking = riderRace.PcsRanking
            },
            Stars = gridEntry.Stars,
            YouthRider = gridEntry.Youth, 
            RiderType = gridEntry.RiderType.ToString()
        };
    }
}

