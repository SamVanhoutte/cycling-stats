using System.Globalization;
using CyclingStats.Logic;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace WorldcyclingStats;

public class WcsStatsCollector : IDataRetriever
{
    private readonly WcsOptions wcsSettings;

    public WcsStatsCollector(IOptions<WcsOptions> wcsOptions)
    {
        wcsSettings = wcsOptions.Value;
    }

    private string BaseUri => wcsSettings.BaseUri;

    public async Task<List<RacerRaceResult>> GetRaceResultsAsync(string raceId, int year, string? stageId = null, int top = 50)
    {
        var race = new RaceDetails();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync($"{BaseUri}/race/{raceId}/{year}/results");


        // Find the table that contains the results
        var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='result']");
        var resultTableNode = resultDivNode.SelectSingleNode("table");

        // Get the rows of the table
        var rows = resultTableNode.SelectNodes("tr")?.Skip(1).Take(top);
        var results = new List<RacerRaceResult>();

        var position = 1;
        // Iterate through each row
        foreach (var row in rows)
        {
            // Get the cells of the row
            var cells = row.SelectNodes("td");
            results.Add(new RacerRaceResult
            {
                Rider = new Rider
                {
                    Name = cells[3].GetInnerText(), Team = cells[6].GetInnerText(),
                    Id = cells[3].SelectNodes("a").FirstOrDefault().GetAttributeText( "href")
                },
                Position = position,
                DelaySeconds = position == 1 ? 0 : cells[7].GetInnerText().ParseTimeDelay()
            });
            position++;
        }

        return results;
    }
    
    public async Task<List<RacerRacePoint>> GetRacePointsAsync(string raceId, int year, string? stageId = null, int top = 50)
    {
        var race = new RaceDetails();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync($"{BaseUri}/game/{raceId}/{year}/statistics/most-points");


        // Find the table that contains the results
        var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='general']");
        var resultTableNode = resultDivNode.SelectSingleNode("table");

        // Get the rows of the table
        var rows = resultTableNode.SelectNodes("tr")?.Skip(1).Take(top);
        var results = new List<RacerRacePoint>();

        var position = 1;
        // Iterate through each row
        foreach (var row in rows)
        {
            // Get the cells of the row
            var cells = row.SelectNodes("td");
            results.Add(new RacerRacePoint
            {
                Rider = new Rider
                {
                    Name = cells[1].GetInnerText(), Team = cells[4].GetInnerText(),
                    Id = cells[1].SelectNodes("a").FirstOrDefault().GetAttributeText( "href")
                },
                Position = position,
                Points = cells[9].InnerText.ParseInteger() ?? 0,
                Stars =     cells[6].SelectNodes("i").Count,
                //DelaySeconds = position == 1 ? 0 : cells[7].GetInnerText().ParseTimeDelay()
            });
            position++;
        }

        return results;
    }

    public async Task<Rider> GetRiderAsync(string riderId)
    {
        var rider = new Rider();
        var web = new HtmlWeb();
        var riderUrl = $"{BaseUri}/rider/{riderId}";
        var doc = await web.LoadFromWebAsync(riderUrl);
        var teamsNode =
            doc.DocumentNode.SelectSingleNode(
                "//div[@class='box box_right']/div[@class='box-content']/table/tr[@class='uneven']");
        rider.Id = riderId;
        rider.Team = teamsNode.SelectNodes("td").Last().GetInnerText();
        rider.Name = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
        rider.Sprinter = GetDiscipline(doc, "Sprinter");
        rider.OneDay = GetDiscipline(doc, "One-day specialist");
        rider.AllRounder = GetDiscipline(doc, "All-rounder");
        rider.TimeTrialist = GetDiscipline(doc, "Time-trialist");
        rider.Puncheur = GetDiscipline(doc, "Puncheur");
        rider.Climber = GetDiscipline(doc, "Climber");
        return rider;
    }

    public async Task<IEnumerable<Rider>> GetTeamRidersAsync(string teamName)
    {
        var web = new HtmlWeb();
        var teamUrl = $"{BaseUri}/team/{teamName}/{DateTime.Now.Year}";
        var doc = await web.LoadFromWebAsync(teamUrl);
        // Check if right page
        var title = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
        if (title.Equals("World Cycling Stats"))
        {
            Console.WriteLine($"Team {teamName} has no page found");
            return new List<Rider>();
        }

        var teamsTableNode =
            doc.DocumentNode.SelectSingleNode(
                "//div[@class='main animated fadeInRight']/div/div[@class='box-content']/table");
        var rows = teamsTableNode.SelectNodes("tr");
        var riders = new List<Rider> { };
        foreach (var riderRow in rows.Skip(1))
        {
            var cols = riderRow.SelectNodes("td");
            var riderCol = cols[1];
            var riderId = Rider.GetRiderIdFromUrl(riderCol.SelectSingleNode("a").GetAttributeText("href"));
            riders.Add(await GetRiderAsync(riderId));
        }

        return riders;
    }

    public Task<IEnumerable<CyclingTeam>> GetTeamsAsync()
    {
        throw new NotImplementedException();
    }

    private int GetDiscipline(HtmlDocument doc, string discipline)
    {
        var disciplineDiv = doc.DocumentNode.SelectSingleNode($"//div[text()='&nbsp;{discipline}']");
        var value = disciplineDiv.GetAttributeText( "aria-valuenow");
        if (string.IsNullOrEmpty(value)) return 0;
        var percentage = double.Parse(value, CultureInfo.InvariantCulture);
        return (int)(Math.Round(percentage));
    }

    public async Task<RaceDetails> GetRaceDataAsync(string raceId, int year, string? stageId = null)
    {
        var race = new RaceDetails();
        var web = new HtmlWeb();
        var uri = $"{BaseUri}/race/{raceId}/{year}";
        var doc = await web.LoadFromWebAsync(uri);

        var title = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
        // Find the table that contains the results
        var routeDivNode = doc.DocumentNode.SelectSingleNode("//div[@class='box box_middle']");
        var secondDiv = routeDivNode.SelectSingleNode("div/div[@class='box-content']");

        // Get the rows of the table
        var rows = secondDiv.SelectNodes("table/tr");
        var result = new RaceDetails
        {
            Id = raceId,
            Name = title,
            Date = rows[0].SelectSingleNode("td").GetInnerText().ParseDateFromText(),
            Distance = rows[4].SelectNodes("td").Last().GetInnerText().ParseDistance(),
            RaceType = rows[1].SelectNodes("td").Last().GetInnerText(),
        };
        return result;
    }
    

    public static List<string> TeamNames => new List<string>
    {
        "groupama-fdj", "team-arkéa-samsic", "ag2r-citroen-team", "cofidis", "ef-education---easypost",
        "team-totalenergies", "israel---premier-tech", "uno-x-pro-cycling-team", "lotto---dstny",
        "intermarché---circus---wanty", "alpecin---deceuninck", "bingoal---wb", "bora-hansgrohe",
        "tudor-pro-cycling-team", "uae-team-emirates", "ineos-grenadiers", "nice-métropole-côte-d'azur",
        "team-flanders---baloise", "trek-segafredo", "quick-step-alpha-vinyl-team", "astana-qazaqstan-team",
        "bahrain-victorious", "bardiani-csf-faizane", "jumbo-visma", "movistar-team", "team-bikeexchange-jayco",
        "team-dsm"
    };
}