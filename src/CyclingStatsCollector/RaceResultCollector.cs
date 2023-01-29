using CyclingStatsCollector.Models;
using HtmlAgilityPack;
using System.Net;


namespace CyclingStatsCollector;

public class RaceResultCollector
{
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
                    Id = GetAttributeText( cells[3].SelectNodes("a").FirstOrDefault(), "href")
                },
                Position = position,
                DelaySeconds = position==1 ? 0 : ParseTimeDelay( GetInnerText(cells[7]))
            });
            position++;
        }

        return results;
    }

    public async Task<RaceResults> GetRaceDataAsync(string raceUrl)
    {
        var race = new RaceResults();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(raceUrl);

        var title = GetInnerText( doc.DocumentNode.SelectSingleNode("//h1"));
        // Find the table that contains the results
        var routeDivNode = doc.DocumentNode.SelectSingleNode("//div[@class='box box_left']");
        var secondDiv = routeDivNode.SelectNodes("div")[1];

        // Get the rows of the table
        var rows = secondDiv.SelectNodes("table/tr");
        var result = new RaceResults
        {
            Name =title,
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
        return node == null ? "" : 
            HtmlEntity.DeEntitize(node.InnerText).TrimStart().TrimEnd();
    }
    
    private string GetAttributeText(HtmlNode? node, string attributeName)
    {
        return node == null ? "" : 
            HtmlEntity.DeEntitize(node.GetAttributeValue(attributeName, "")).TrimStart().TrimEnd();
    }
}