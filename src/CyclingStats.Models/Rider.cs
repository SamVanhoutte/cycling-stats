namespace CyclingStats.Models;

public class Rider
{
    private string id;

    public string Id
    {
        get => id;
        set => id = GetRiderIdFromUrl(value);
    }

    public static string GetRiderIdFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        var paths = url.Split("/");
        if (int.TryParse(paths.Last(), out _))
        {
            return paths.SkipLast(1).Last();
        }
        return paths.Last();
    }

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
    public bool ContainsProfile => Puncheur >= 0 || Sprinter >= 0 || GC >= 0 || OneDay >= 0 || Climber >= 0 || TimeTrialist >= 0;
    public bool DetailsCompleted { get; set; }
    public string PcsRiderId => PcsId ?? Id;
    public RiderStatus? Status { get; set; }

    public static int ParseBirthYear(string? birthDayLine)
    {
        // The format is : Date of birth: 21st September 1998 (26)
        var parts = birthDayLine.Split(' ');
        if (parts.Length > 2)
        {
            if (int.TryParse(parts.SkipLast(1).Last().Trim(), out var year))
            {
                return year;
            }
        }

        return -1;
    }
}