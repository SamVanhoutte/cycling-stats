// using System.Globalization;
// using CyclingStats.Logic;
// using CyclingStats.Logic.Configuration;
// using CyclingStats.Logic.Interfaces;
// using CyclingStats.Models;
// using HtmlAgilityPack;
// using Microsoft.Extensions.Options;
//
// namespace ProcyclingStats;
//
//
// public class PcsStatsCollector : IDataRetriever
// {
//     private readonly PcsOptions pcsSettings;
//
//     public PcsStatsCollector(IOptions<PcsOptions> wcsOptions)
//     {
//         pcsSettings = wcsOptions.Value;
//     }
//
//     private string BaseUri => pcsSettings.BaseUri;
//
//     public async Task<List<RacerRaceResult>> GetRaceResultsAsync(string raceId, int year, string? stageId = null, int top = 50)
//     {
//         var race = new RaceDetails();
//         var web = new HtmlWeb();
//         var doc = await web.LoadFromWebAsync($"{BaseUri}/race/{raceId}");
//
//
//         // Find the table that contains the results
//         var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='result']");
//         var resultTableNode = resultDivNode.SelectSingleNode("table");
//
//         // Get the rows of the table
//         var rows = resultTableNode.SelectNodes("tr")?.Skip(1).Take(top);
//         var results = new List<RacerRaceResult>();
//
//         var position = 1;
//         // Iterate through each row
//         foreach (var row in rows)
//         {
//             // Get the cells of the row
//             var cells = row.SelectNodes("td");
//             results.Add(new RacerRaceResult
//             {
//                 Rider = new Rider
//                 {
//                     Name = GetInnerText(cells[3]), Team = GetInnerText(cells[6]),
//                     Id = GetAttributeText(cells[3].SelectNodes("a").FirstOrDefault(), "href")
//                 },
//                 Position = position,
//                 DelaySeconds = position == 1 ? 0 : ParseTimeDelay(GetInnerText(cells[7]))
//             });
//             position++;
//         }
//
//         return results;
//     }
//
//     public Task<List<RacerRacePoint>> GetRacePointsAsync(string raceId, int year, string? stageId = null, int top = 50)
//     {
//         throw new NotImplementedException();
//     }
//
//     
//
//     public async Task<IEnumerable<Rider>> GetTeamRidersAsync(string teamName)
//     {
//         var web = new HtmlWeb();
//         var teamUrl = $"{BaseUri}/team/{teamName}/{DateTime.Now.Year}";
//         var doc = await web.LoadFromWebAsync(teamUrl);
//         // Check if right page
//         var title = GetInnerText(doc.DocumentNode.SelectSingleNode("//h1"));
//         if (title.Equals("World Cycling Stats"))
//         {
//             Console.WriteLine($"Team {teamName} has no page found");
//             return new List<Rider>();
//         }
//
//         var teamsTableNode =
//             doc.DocumentNode.SelectSingleNode(
//                 "//div[@class='main animated fadeInRight']/div/div[@class='box-content']/table");
//         var rows = teamsTableNode.SelectNodes("tr");
//         var riders = new List<Rider> { };
//         foreach (var riderRow in rows.Skip(1))
//         {
//             var cols = riderRow.SelectNodes("td");
//             var riderCol = cols[1];
//             var riderId = Rider.GetRiderIdFromUrl(GetAttributeText(riderCol.SelectSingleNode("a"), "href"));
//             riders.Add(await GetRiderAsync(riderId));
//         }
//
//         return riders;
//     }
//
//     public Task<IEnumerable<CyclingTeam>> GetTeamsAsync()
//     {
//         throw new NotImplementedException();
//     }
//
//
//     
//
//     
//
//     public Task<IDictionary<int, RaceDetails>> GetPastRaceResultsAsync(string raceId, int years, string? stageId = null)
//     {
//         throw new NotImplementedException();
//     }
//
//     private decimal parseDistance(string distanceText)
//     {
//         return decimal.Parse(distanceText.Split(" ").First(), CultureInfo.InvariantCulture);
//     }
//
//     private int ParseTimeDelay(string timeDelay)
//     {
//         if (string.IsNullOrWhiteSpace(timeDelay)) return -1;
//         timeDelay = timeDelay.Replace("+", "");
//         timeDelay = timeDelay.Replace(" ", "");
//         timeDelay = timeDelay.TrimStart().TrimEnd();
//         var minutes = int.Parse(timeDelay.Split("'")[0]);
//         var seconds = int.Parse((timeDelay.Split("'")[1]).Replace("'", ""));
//         return minutes * 60 + seconds;
//     }
//
//     private string GetInnerText(HtmlNode? node)
//     {
//         return node == null ? "" : HtmlEntity.DeEntitize(node.InnerText).TrimStart().TrimEnd();
//     }
//
//     private string GetAttributeText(HtmlNode? node, string attributeName)
//     {
//         return node == null
//             ? ""
//             : HtmlEntity.DeEntitize(node.GetAttributeValue(attributeName, "")).TrimStart().TrimEnd();
//     }
//
//     public static List<string> TeamNames => new List<string>
//     {
//         "groupama-fdj", "team-arkéa-samsic", "ag2r-citroen-team", "cofidis", "ef-education---easypost",
//         "team-totalenergies", "israel---premier-tech", "uno-x-pro-cycling-team", "lotto---dstny",
//         "intermarché---circus---wanty", "alpecin---deceuninck", "bingoal---wb", "bora-hansgrohe",
//         "tudor-pro-cycling-team", "uae-team-emirates", "ineos-grenadiers", "nice-métropole-côte-d'azur",
//         "team-flanders---baloise", "trek-segafredo", "quick-step-alpha-vinyl-team", "astana-qazaqstan-team",
//         "bahrain-victorious", "bardiani-csf-faizane", "jumbo-visma", "movistar-team", "team-bikeexchange-jayco",
//         "team-dsm"
//     };
// }