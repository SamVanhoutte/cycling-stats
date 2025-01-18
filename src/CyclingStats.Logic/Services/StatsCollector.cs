using System.Globalization;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Services;

public class StatsCollector : IDataRetriever
{
    private string WcsBaseUri => wcsSettings.BaseUri;
    private string PcsBaseUri => pcsSettings.BaseUri;

    private readonly WcsOptions wcsSettings;
    private readonly PcsOptions pcsSettings;

    public StatsCollector(IOptions<WcsOptions> wcsOptions, IOptions<PcsOptions> pcsOptions)
    {
        wcsSettings = wcsOptions.Value;
        pcsSettings = pcsOptions.Value;
    }

    public async Task<List<RacerRaceResult>> GetRaceResultsAsync(string raceId, int year, string? stageId = null,
        int top = 50)
    {
        var race = new RaceDetails();
        var web = new HtmlWeb();
        var pageUrl = string.IsNullOrEmpty(stageId) ?
            $"{WcsBaseUri}/race/{raceId}/{year}/results":
            $"{WcsBaseUri}/race/{raceId}/{year}/{stageId}/results";
        var doc = await web.LoadFromWebAsync(pageUrl);


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
                    Id = cells[3].SelectNodes("a").FirstOrDefault().GetAttributeText("href")
                },
                Position = position,
                DelaySeconds = position == 1 ? 0 : cells[7].GetInnerText().ParseTimeDelay()
            });
            position++;
        }

        return results;
    }

    public async Task<List<RacerRacePoint>> GetRacePointsAsync(string raceId, int year, string? stageId = null,
        int top = 50)
    {
        var race = new RaceDetails();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync($"{WcsBaseUri}/game/{raceId}/{year}/statistics/most-points");


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
                    Id = cells[1].SelectNodes("a").FirstOrDefault().GetAttributeText("href")
                },
                Position = position,
                Points = cells[9].InnerText.ParseInteger() ?? 0,
                Stars = cells[6].SelectNodes("i").Count,
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
        var riderUrl = $"{WcsBaseUri}/rider/{riderId}";
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
        var teamUrl = $"{WcsBaseUri}/team/{teamName}/{DateTime.Now.Year}";
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

    public async Task<RaceDetails> GetRaceDataAsync(string raceId, int year, string? stageId = null)
    {
        var web = new HtmlWeb();
        var uri = string.IsNullOrEmpty(stageId)
            ? $"{PcsBaseUri}/race/{raceId}/{year}/result"
            : $"{PcsBaseUri}/race/{raceId}/{year}/{stageId}/result";
        var doc = await web.LoadFromWebAsync(uri);


        var infoItems = doc.DocumentNode.SelectNodes("//ul[@class='infolist']/li");
        // Get the rows of the table
        var race = new RaceDetails
        {
            Name = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText(), StageId = stageId
        };
        var canceledElement = doc.DocumentNode.SelectSingleNode("//div[@class='red fs16']");
        if (canceledElement != null)
        {
            if (canceledElement.GetInnerText().Contains("cancelled", StringComparison.InvariantCultureIgnoreCase))
            {
                race.Status = RaceStatus.Canceled;
                return race;
            }
        }

        foreach (var infoItem in infoItems)
        {
            try
            {
                if (infoItem.SelectNodes("div").Any())
                {
                    var infoTitle = infoItem.SelectNodes("div").First().GetInnerText()?.Replace(":", "")?.Trim()
                        ?.ToLower();
                    var infoField = infoItem.SelectNodes("div").Last().GetInnerText();
                    switch (infoTitle)
                    {
                        case "date":
                            race.Date = infoField.ParseDateFromText();
                            break;
                        case "distance":
                            race.Distance = infoField.ParseDistance();
                            break;
                        case "vertical meters":
                            race.Elevation = int.Parse(infoField);
                            break;
                        case "points scale":
                            race.PointsScale = infoField;
                            race.RaceType = infoField.EndsWith(".stage", StringComparison.InvariantCultureIgnoreCase)
                                ? "Multi-day race"
                                : "One-day race";
                            if (string.IsNullOrEmpty(race.StageId) && (race.StageRace ?? false))
                            {
                                var stageTitle = doc.DocumentNode
                                    .SelectSingleNode("//div[@class='page-title']/div[@class='sub']/div[@class='blue']")
                                    .GetInnerText();
                                if (!string.IsNullOrEmpty(stageTitle))
                                {
                                    race.StageId = stageTitle.ToLower().Replace(" ", "-");
                                }
                            }

                            break;
                        case "uci scale": race.UciScale = infoField; break;
                        case "profilescore": race.ProfileScore = infoField.ParseInteger(); break;
                        case "startlist quality score": race.StartlistQuality = infoField.ParseInteger(); break;
                        case "race ranking": race.RaceRanking = infoField.ParseInteger(); break;
                        case "classification": race.Classification = infoField; break;
                        case "race category": race.Category = infoField; break;
                        case "won how": race.DecidingMethod = infoField?.Replace(" ", ""); break;
                        case "parcours type":
                            var profileClass = infoItem.SelectSingleNode(".//span").GetAttributeText("class");
                            if (!string.IsNullOrEmpty(profileClass))
                            {
                                race.ParcoursType = int.Parse(profileClass.Last().ToString());
                            }

                            break;
                        default: break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        var mt10Node = doc.DocumentNode.SelectSingleNode($"//div[@class='mt10']")?.SelectSingleNode(".//img");
        if (mt10Node != null)
        {
            race.ProfileImageUrl = $"{PcsBaseUri}/{mt10Node.GetAttributeText("src")}";
        }

        return race;
    }

    public async Task<IDictionary<int, RaceDetails>> GetPastRaceResultsAsync(string raceId, int years, string? stageId = null)
    {
        var fullResultSet = new Dictionary<int, RaceDetails> { }; 
        for (var yearToCheck = DateTime.Now.Year; yearToCheck > DateTime.Now.Year - years; yearToCheck--)
        {
            var raceDetails = await GetRaceDataAsync(raceId, yearToCheck, stageId);
            raceDetails.Results = await GetRaceResultsAsync(raceId, yearToCheck, stageId, -1);
            raceDetails.Points = await GetRacePointsAsync(raceId, yearToCheck, stageId, -1);
            fullResultSet.Add(yearToCheck, raceDetails);
        }

        return fullResultSet;
    }
    
    
    
    // public async Task<RaceDetails> GetWcsRaceDataAsync(string raceId, int year, string? stageId = null)
    // {
    //     var race = new RaceDetails();
    //     var web = new HtmlWeb();
    //     var uri = $"{BaseUri}/race/{raceId}/{year}";
    //     var doc = await web.LoadFromWebAsync(uri);
    //
    //     var title = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
    //     // Find the table that contains the results
    //     var routeDivNode = doc.DocumentNode.SelectSingleNode("//div[@class='box box_middle']");
    //     var secondDiv = routeDivNode.SelectSingleNode("div/div[@class='box-content']");
    //
    //     // Get the rows of the table
    //     var rows = secondDiv.SelectNodes("table/tr");
    //     var result = new RaceDetails
    //     {
    //         Id = raceId,
    //         Name = title,
    //         Date = rows[0].SelectSingleNode("td").GetInnerText().ParseDateFromText(),
    //         Distance = rows[4].SelectNodes("td").Last().GetInnerText().ParseDistance(),
    //         RaceType = rows[1].SelectNodes("td").Last().GetInnerText(),
    //     };
    //     return result;
    // }

    private int GetDiscipline(HtmlDocument doc, string discipline)
    {
        var disciplineDiv = doc.DocumentNode.SelectSingleNode($"//div[text()='&nbsp;{discipline}']");
        var value = disciplineDiv.GetAttributeText("aria-valuenow");
        if (string.IsNullOrEmpty(value)) return 0;
        var percentage = double.Parse(value, CultureInfo.InvariantCulture);
        return (int)(Math.Round(percentage));
    }

}