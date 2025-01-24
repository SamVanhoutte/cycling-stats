using System.Globalization;
using CyclingStats.DataAccess.Entities;
using CyclingStats.Logic.Configuration;
using CyclingStats.Logic.Exceptions;
using CyclingStats.Logic.Extensions;
using CyclingStats.Logic.Interfaces;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Rider = CyclingStats.Models.Rider;

namespace CyclingStats.Logic.Services;

public class StatsCollector : IDataRetriever
{
    private string WcsBaseUri => wcsSettings.BaseUri.TrimEnd('/');
    private string PcsBaseUri => pcsSettings.BaseUri.TrimEnd('/');

    private readonly WcsOptions wcsSettings;
    private readonly PcsOptions pcsSettings;

    public StatsCollector(IOptions<WcsOptions> wcsOptions, IOptions<PcsOptions> pcsOptions)
    {
        wcsSettings = wcsOptions.Value;
        pcsSettings = pcsOptions.Value;
    }

    public async Task<List<RacerRaceResult>> GetRaceResultsAsync(Race raceInfo, int top = 50)
    {
        var race = new RaceDetails();
        var web = new HtmlWeb();
        var pageUrl = $"{WcsBaseUri}/race/{raceInfo.Id}/results";
        var doc = await web.LoadFromWebAsync(pageUrl);


        // Find the table that contains the results
        var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='result']");
        if (resultDivNode == null)
        {
            throw new NoResultsAvailableException();
        }

        var resultTableNode = resultDivNode.SelectSingleNode("table");

        var points = resultTableNode.ParseTable<RacerRaceResult>((row, position) =>
            new RacerRaceResult
            {
                Rider = new Rider
                {
                    Name = row["Rider"].GetInnerText(), Team = row["Team"].GetInnerText(),
                    Id = row["Rider"].SelectNodes("a").FirstOrDefault().GetAttributeText("href")
                },
                Position = position,
                DelaySeconds = position == 1 ? 0 : row["Gap"].GetInnerText().ParseTimeDelay()
            }, [2, 4, 7], top: top);

        return points;
    }

    public async Task<List<RacerRacePoint>> GetRacePointsAsync(Race raceInfo, int top = 50)
    {
        return raceInfo.IsStageRace
            ? await GetStageRacePointsAsync(raceInfo, top)
            : await GetOneDayRacePointsAsync(raceInfo, top);
    }

    private async Task<List<RacerRacePoint>> GetStageRacePointsAsync(Race raceInfo, int top)
    {
        var web = new HtmlWeb();
        var url = $"{WcsBaseUri}/race/{raceInfo.Id}/result/game";
        var doc = await web.LoadFromWebAsync(url);

        // First check if results are yet in, or not
        if (doc.ContainsText(StringComparison.InvariantCulture, "Lockdown", "No points available"))
        {
            throw new NoResultsAvailableException();
        }

        // Find the table that contains the results
        var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='result']");
        if (resultDivNode == null)
        {
            throw new GameNotEnabledException();
        }

        var resultTableNode = resultDivNode.SelectSingleNode("table");

        var results = resultTableNode.ParseTable<RacerRacePoint>((row, position) =>
            new RacerRacePoint
            {
                Rider = new Rider
                {
                    Name = row["Rider"].GetInnerText(), Team = row["Team"].GetInnerText(),
                    Id = row["Rider"].SelectNodes("a").FirstOrDefault().GetAttributeText("href")
                },
                Position = position,
                Stars = row["Stars"].SelectNodes("i").Count,
                Picked = row["%"].GetInnerText().ParsePercentage() ?? 0,
                Points = row["Pts"].GetInnerText().ParseInteger() ?? 0,
            }, [3], top: top);

        if (!results.Any())
        {
            throw new NoResultsAvailableException();
        }

        return results;
    }

    private async Task<List<RacerRacePoint>> GetOneDayRacePointsAsync(Race raceInfo, int top)
    {
        var web = new HtmlWeb();
        var url = $"{WcsBaseUri}/game/{raceInfo.Id}/statistics/most-points";
        var doc = await web.LoadFromWebAsync(url);

        // First check if results are yet in, or not
        if (doc.ContainsText(StringComparison.InvariantCulture, "Lockdown", "No points available"))
        {
            throw new NoResultsAvailableException();
        }

        // Find the table that contains the results
        var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='general']");
        if (resultDivNode == null)
        {
            throw new GameNotEnabledException();
        }

        var resultTableNode = resultDivNode.SelectSingleNode("table");

        var results = resultTableNode.ParseTable<RacerRacePoint>((row, position) =>
            new RacerRacePoint
            {
                Rider = new Rider
                {
                    Name = row["Rider"].GetInnerText(), Team = row["Team"].GetInnerText(),
                    Id = row["Rider"].SelectNodes("a").FirstOrDefault().GetAttributeText("href")
                },
                Position = position,
                Stars = row["Stars"].SelectNodes("i").Count,
                Picked = row["%"].GetInnerText().ParsePercentage() ?? 0,
                Points = row["Pts"].GetInnerText().ParseInteger() ?? 0,
                Pc = row["PC"].InnerText.ParseInteger() ?? 0,
                Mc = row["MC"].InnerText.ParseInteger() ?? 0,
                Gc = row["GC"].InnerText.ParseInteger() ?? 0,
            }, [3], top: top);
        return results;
    }

    public async Task<Rider?> GetRiderProfileAsync(Models.Rider riderToQuery)
    {
        var web = new HtmlWeb();
        var riderUrl = $"{PcsBaseUri}/rider/{riderToQuery.PcsRiderId}";
        var doc = await web.LoadFromWebAsync(riderUrl);
        var infoNode = doc.DocumentNode.SelectSingleNode("//div[@class='rdr-info-cont']");
        var rider = new Models.Rider
        {
            Id = riderToQuery.Id, PcsId = riderToQuery.PcsId, Status = riderToQuery.Status, Height = -1, Weight = -1,
            PcsRanking = -1, UciRanking = -1
        };
        // Get name from h1 node
        rider.Name = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
        if (rider.Name.Contains("not found", StringComparison.InvariantCultureIgnoreCase)) return null;
        // Get team from the first span node
        var mainDivNode = doc.DocumentNode.SelectSingleNode("//div[@class='main']");
        rider.Team = mainDivNode.ChildNodes?.LastOrDefault()?.InnerText;
        if (rider.Team.Equals(rider.Name))
        {
            rider.Status = RiderStatus.Retired;
            rider.Team = "No team";
        }

        // Birth year is of the first line
        var lines = infoNode.ParseLines();
        rider.BirthYear = Rider.ParseBirthYear(lines.FirstOrDefault());
        // Now stepping to the next <span> node and taking the weight
        var nextSpan = infoNode.ChildNodes.Last();
        var nextLines = nextSpan.ParseLines("span");
        rider.Weight = nextLines?.FirstOrDefault().GetIntPart() ?? -1;

        // Now stepping one level deeper and taking the height
        if (nextSpan?.ChildNodes.Any() ?? false)
        {
            nextSpan = nextSpan.ChildNodes.Last();
            lines = nextSpan.ParseLines();
            rider.Height = (int)(100 * lines?.FirstOrDefault().GetDecimalPart() ?? -1);
        }

        // Up to the points per speciality div now
        var ppsNode = doc.DocumentNode.SelectSingleNode("//div[@class='pps']");
        foreach (var specialityNode in ppsNode.SelectNodes(".//li"))
        {
            var speciality = specialityNode.SelectSingleNode("div[@class='title']").GetInnerText();
            var points = specialityNode.SelectSingleNode("div[@class='pnt']").InnerText.ParseInteger() ?? -1;
            switch (speciality.ToLower())
            {
                case "onedayraces": rider.OneDay = points; break;
                case "gc": rider.GC = points; break;
                case "tt": rider.TimeTrialist = points; break;
                case "sprint": rider.Sprinter = points; break;
                case "climber": rider.Climber = points; break;
                case "hills": rider.Puncheur = points; break;
                default: break;
            }
        }

        // Get PCS & UCI ranking
        var rankingsNodes = doc.DocumentNode.SelectNodes("//ul[@class='list horizontal rdr-rankings']/li");
        if (rankingsNodes != null)
        {
            rider.UciRanking = rankingsNodes.First().ChildNodes[1].InnerText.ParseInteger() ?? -1;
            rider.PcsRanking = rankingsNodes.Last().ChildNodes[1].InnerText.ParseInteger() ?? -1;
        }

        // Get rankings of past years
        var seasonsTable =
            doc.DocumentNode.SelectSingleNode("//table[@class='basic rdr-season-stats']"); // Getting table node
        var seasonResults = seasonsTable.ParseTable((row, position) => new
        {
            Year = row["Column1"].InnerText.ParseInteger(),
            Points = row["Points"].InnerText.ParseInteger()
        }, thHeader: true);
        rider.Ranking2019 = seasonResults?.FirstOrDefault(sr => sr.Year == 2019)?.Points ?? -1;
        rider.Ranking2020 = seasonResults?.FirstOrDefault(sr => sr.Year == 2020)?.Points ?? -1;
        rider.Ranking2021 = seasonResults?.FirstOrDefault(sr => sr.Year == 2021)?.Points ?? -1;
        rider.Ranking2022 = seasonResults?.FirstOrDefault(sr => sr.Year == 2022)?.Points ?? -1;
        rider.Ranking2023 = seasonResults?.FirstOrDefault(sr => sr.Year == 2023)?.Points ?? -1;
        rider.Ranking2024 = seasonResults?.FirstOrDefault(sr => sr.Year == 2024)?.Points ?? -1;
        rider.Ranking2025 = seasonResults?.FirstOrDefault(sr => sr.Year == 2025)?.Points ?? -1;
        rider.Ranking2026 = seasonResults?.FirstOrDefault(sr => sr.Year == 2026)?.Points ?? -1;
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
            riders.Add(await GetRiderProfileAsync(new Rider { Id = riderId }));
        }

        return riders;
    }

    public Task<IEnumerable<CyclingTeam>> GetTeamsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<RaceDetails>> GetRaceDataAsync(Race race)
    {
        var races = new List<RaceDetails> { };
        var isStageRace = await IsStageRaceAsync(race);
        if (isStageRace)
        {
            var web = new HtmlWeb();
            var racePageUri = $"{PcsBaseUri}/race/{race.PcsRaceId}";
            var doc = await web.LoadFromWebAsync(racePageUri);
            var stageRaceNode = doc.DocumentNode.SelectSingleNode("//h3[text()='Stages']");
            var stageTable = stageRaceNode.SelectSingleNode("..//table");

            races = stageTable.ParseTable<RaceDetails>((row, position) =>
            {
                if (string.IsNullOrEmpty(row["Stage"].GetInnerText()))
                {
                    return null;
                }

                var link = row["Stage"].SelectSingleNode("a").GetAttributeText("href");
                var stageId = link.Split("/").Last();
                return new RaceDetails
                {
                    Id = string.IsNullOrEmpty(stageId) ? race.Id : $"{race.Id}/{stageId}",
                    PcsId = RaceDetails.GetRaceIdFromUrl(link),
                    Date = row["Date"].GetInnerText().ParseDateFromText(race.Id.GetYearFromRaceId()),
                    Name = row["Stage"].SelectSingleNode("a").GetInnerText(),
                    Distance = row["KM"].GetInnerText().ParseDecimal(),
                    Status = RaceStatus.New,
                };
            }, [], thHeader: true);

            return races;
        }

        var details = await GetDayRaceDataAsync(race);
        return [details];
    }

    private async Task<RaceDetails> GetDayRaceDataAsync(Race raceInfo)
    {
        var web = new HtmlWeb();
        var racePageUri = $"{PcsBaseUri}/race/{raceInfo.PcsRaceId}/result";
        var doc = await web.LoadFromWebAsync(racePageUri);

        var pageTitle = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
        if (pageTitle.Equals("page not found", StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        var infoItems = doc.DocumentNode.SelectNodes("//ul[@class='infolist']/li");
        // Get the rows of the table
        var race = new RaceDetails
        {
            Name = pageTitle, Id = raceInfo.Id, PcsId = raceInfo.PcsRaceId,
            PcsUrl = $"{PcsBaseUri}/race/{raceInfo.PcsRaceId}", WcsUrl = $"{WcsBaseUri}/race/{raceInfo.Id}",
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
                            race.Date = infoField.ParseDateFromText(raceInfo.Id.GetYearFromRaceId());
                            break;
                        case "distance":
                            race.Distance = infoField.ParseDistance();
                            break;
                        case "vertical meters":
                            race.Elevation = infoField.ParseInteger();
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

    private async Task<bool> IsStageRaceAsync(Race race)
    {
        var web = new HtmlWeb();
        var racePageUri = $"{PcsBaseUri}/race/{race.PcsRaceId}";
        var doc = await web.LoadFromWebAsync(racePageUri);
        var stageRaceNode = doc.DocumentNode.SelectSingleNode("//h3[text()='Stages']");
        return stageRaceNode != null;
    }

    public async Task<IDictionary<int, RaceDetails>> GetPastRaceResultsAsync(Race race, int years,
        string? stageId = null)
    {
        //TODO : years should be considered in the loops
        throw new NotImplementedException();
        // var fullResultSet = new Dictionary<int, RaceDetails> { };
        // for (var yearToCheck = DateTime.Now.Year; yearToCheck > DateTime.Now.Year - years; yearToCheck--)
        // {
        //     var results = await GetRaceDataAsync(race, stageId);
        //     var raceDetails = results.Single();
        //     raceDetails.Results = await GetRaceResultsAsync(race.Id, stageId, -1);
        //     raceDetails.Points = await GetRacePointsAsync(race.Id, yearToCheck, stageId, -1);
        //     fullResultSet.Add(yearToCheck, raceDetails);
        // }
        //
        // return fullResultSet;
    }

    public async Task<List<RaceDetails>> GetRaceCalendarAsync(int year)
    {
        var webUrl = $"{WcsBaseUri}/races/{year}/calendar";
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(webUrl);

        var raceTableNode = doc.DocumentNode.SelectSingleNode("//table[@class='large']");

        var races = raceTableNode.ParseTable<RaceDetails>((row, position) =>
            new RaceDetails
            {
                Id = RaceDetails.GetRaceIdFromUrl(row["Race"].SelectSingleNode("a").GetAttributeText("href"), year),
                Name = row["Race"].SelectSingleNode("a").GetInnerText(),
                Status = RaceStatus.New,
            }, [4, 6]);
        return races;
    }

    public async Task<string?> GetPcsRaceIdAsync(Race race)
    {
        var raceYear = race.Id.GetYearFromRaceId();
        //https://www.procyclingstats.com/search.php?term=Acht+van+Bladel
        var searchUrl = $"{PcsBaseUri}/search.php?term={race.Name.Replace(" ", "+")}";
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(searchUrl);

        var searchNode = doc.DocumentNode.SelectSingleNode("//h3[text()='Search results']");
        var searchResults = searchNode.SelectNodes("../ul/li");
        foreach (var searchResult in searchResults)
        {
            var link = searchResult.SelectSingleNode("a").GetAttributeText("href");
            var text = searchResult.SelectSingleNode("a").GetInnerText();
            if (link.Contains("race/", StringComparison.InvariantCultureIgnoreCase) &&
                text.Contains(raceYear.ToString()))
            {
                return RaceDetails.GetRaceIdFromUrl(link);
            }
        }

        return null;
    }

    public async Task<string?> GetPcsRiderIdAsync(Rider rider)
    {
        //https://www.procyclingstats.com/search.php?term=Acht+van+Bladel
        var searchTerm = string.IsNullOrEmpty(rider.Name) ? rider.Id.Replace("-", " ") : rider.Name;
        var searchUrl = $"{PcsBaseUri}/search.php?term={searchTerm.Replace(" ", "+")}";
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(searchUrl);

        var searchNode = doc.DocumentNode.SelectSingleNode("//h3[text()='Search results']");
        var searchResults = searchNode.SelectNodes("../ul/li");
        foreach (var searchResult in searchResults)
        {
            var link = searchResult.SelectSingleNode("a").GetAttributeText("href");
            var text = searchResult.SelectSingleNode("a").GetInnerText();
            if (link.Contains("rider/", StringComparison.InvariantCultureIgnoreCase))
            {
                return Rider.GetRiderIdFromUrl(link);
            }
        }

        return null;
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

    // public async Task<Rider> GetWcsRiderProfileAsync(string riderId)
    // {
    //     var rider = new Rider();
    //     var web = new HtmlWeb();
    //     var riderUrl = $"{WcsBaseUri}/rider/{riderId}";
    //     var doc = await web.LoadFromWebAsync(riderUrl);
    //     var teamsNode =
    //         doc.DocumentNode.SelectSingleNode(
    //             "//div[@class='box box_right']/div[@class='box-content']/table/tr[@class='uneven']");
    //     rider.Id = riderId;
    //     rider.Team = teamsNode.SelectNodes("td").Last().GetInnerText();
    //     rider.Name = doc.DocumentNode.SelectSingleNode("//h1").GetInnerText();
    //     rider.Sprinter = GetDiscipline(doc, "Sprinter");
    //     rider.OneDay = GetDiscipline(doc, "One-day specialist");
    //     rider.GC = GetDiscipline(doc, "All-rounder");
    //     rider.TimeTrialist = GetDiscipline(doc, "GC");
    //     rider.Puncheur = GetDiscipline(doc, "Puncheur");
    //     rider.Climber = GetDiscipline(doc, "Climber");
    //     return rider;
    // }
    // private int GetDiscipline(HtmlDocument doc, string discipline)
    // {
    //     var disciplineDiv = doc.DocumentNode.SelectSingleNode($"//div[text()='&nbsp;{discipline}']");
    //     var value = disciplineDiv.GetAttributeText("aria-valuenow");
    //     if (string.IsNullOrEmpty(value)) return 0;
    //     var percentage = double.Parse(value, CultureInfo.InvariantCulture);
    //     return (int)(Math.Round(percentage));
    // }
}