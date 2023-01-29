using System.Globalization;
using CyclingStatsCollector.Models;
using HtmlAgilityPack;
using System.Net;


namespace CyclingStatsCollector;

public class StatsCollector
{
    private static string BaseUri = "https://www.worldcyclingstats.com/en";

    public async Task<List<RacerRaceResult>> GetRaceResultsAsync(string raceUrl, int top = 50)
    {
        var race = new RaceResults();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(raceUrl);


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
                    Name = GetInnerText(cells[3]), Team = GetInnerText(cells[6]),
                    Id = GetAttributeText(cells[3].SelectNodes("a").FirstOrDefault(), "href")
                },
                Position = position,
                DelaySeconds = position == 1 ? 0 : ParseTimeDelay(GetInnerText(cells[7]))
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
        rider.Team = GetInnerText(teamsNode.SelectNodes("td").Last());
        rider.Name = GetInnerText(doc.DocumentNode.SelectSingleNode("//h1"));
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
        var teamsTableNode =
            doc.DocumentNode.SelectSingleNode(
                "//div[@class='main animated fadeInRight']/div/div[@class='box-content']/table");
        var rows = teamsTableNode.SelectNodes("tr");
        var riders = new List<Rider> { };
        foreach (var riderRow in rows.Skip(1))
        {
            var cols = riderRow.SelectNodes("td");
            var riderCol = cols[1];
            var riderId = Rider.GetRiderIdFromUrl( GetAttributeText(riderCol.SelectSingleNode("a"), "href"));
            riders.Add(await GetRiderAsync(riderId));
        }

        return riders;
    }

    private int GetDiscipline(HtmlDocument doc, string discipline)
    {
        var disciplineDiv = doc.DocumentNode.SelectSingleNode($"//div[text()='&nbsp;{discipline}']");
        var value = GetAttributeText(disciplineDiv, "aria-valuenow");
        if (string.IsNullOrEmpty(value)) return 0;
        var percentage = double.Parse(value, CultureInfo.InvariantCulture);
        return (int) (Math.Round(percentage));
    }

    public async Task<RaceResults> GetRaceDataAsync(string raceUrl)
    {
        var race = new RaceResults();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(raceUrl);

        var title = GetInnerText(doc.DocumentNode.SelectSingleNode("//h1"));
        // Find the table that contains the results
        var routeDivNode = doc.DocumentNode.SelectSingleNode("//div[@class='box box_left']");
        var secondDiv = routeDivNode.SelectNodes("div")[1];

        // Get the rows of the table
        var rows = secondDiv.SelectNodes("table/tr");
        var result = new RaceResults
        {
            Name = title,
            Date = GetInnerText(rows[0].SelectSingleNode("td")),
            RaceType = GetInnerText(rows[1].SelectNodes("td").Last()),
        };
        return result;
    }

    private int ParseTimeDelay(string timeDelay)
    {
        timeDelay = timeDelay.Replace("+", "");
        timeDelay = timeDelay.Replace(" ", "");
        timeDelay = timeDelay.TrimStart().TrimEnd();
        var minutes = int.Parse(timeDelay.Split("'")[0]);
        var seconds = int.Parse((timeDelay.Split("'")[1]).Replace("'", ""));
        return minutes * 60 + seconds;
    }

    private string GetInnerText(HtmlNode? node)
    {
        return node == null ? "" : HtmlEntity.DeEntitize(node.InnerText).TrimStart().TrimEnd();
    }

    private string GetAttributeText(HtmlNode? node, string attributeName)
    {
        return node == null
            ? ""
            : HtmlEntity.DeEntitize(node.GetAttributeValue(attributeName, "")).TrimStart().TrimEnd();
    }
}