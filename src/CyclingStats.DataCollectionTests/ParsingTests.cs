using CyclingStats.Logic.Extensions;
using CyclingStats.Models;
using CyclingStats.Models.Extensions;
using HtmlAgilityPack;

namespace CyclingStats.DataCollectionTests;

public class ParsingTests
{
    [Theory]
    [InlineData("/race/hapert-acht-van-bladel/2025", "hapert-acht-van-bladel/2025")]
    [InlineData("https://www.procyclingstats.com/race/hapert-acht-van-bladel/2025", "hapert-acht-van-bladel/2025")]
    [InlineData("race/hapert-acht-van-bladel/2025/stage-1", "hapert-acht-van-bladel/2025/stage-1")]
    [InlineData("https://www.procyclingstats.com/race/hapert-acht-van-bladel/2025/stage-1",
        "hapert-acht-van-bladel/2025/stage-1")]
    [InlineData("https://www.worldcyclingstats.com/en/race/classica-comunitat-valenciana-1969",
        "classica-comunitat-valenciana-1969/2025")]
    public void TestRaceUrlParsing(string url, string expectedId)
    {
        Assert.Equal(expectedId, RaceDetails.GetRaceIdFromUrl(url));
    }
    
    [Theory]
    [InlineData("59.2%", 59.2)]
    [InlineData("", null)]
    [InlineData("-", null)]
    [InlineData(null, null)]
    public void TestPercentageParsing(string? text, double? expectedValue)
    {
        var expectedDecimal = (decimal?)expectedValue;
        Assert.Equal(expectedDecimal, text.ParsePercentage());
    }
    
    [Theory]
    [InlineData("+ 00' 03''", 3)]
    [InlineData("+ 13' 03''", 783)]
    [InlineData("+1h 02' 21''", 3741)]
    [InlineData("4h 31' 28''", 16288)]
    public void TestParseTime(string delay, int expectedDelay)
    {
        Assert.Equal(expectedDelay, delay.ParseTimeDelay());
    }
    
    [Theory]
    [InlineData("https://www.worldcyclingstats.com/en/race/tour-down-under/2024/stage-2/result/game", "Isaac")]
    public async Task TestTextInPageAsync(string url, string expectedWord)
    {
        var page = new HtmlWeb();
        var doc = await page.LoadFromWebAsync(url);
        Assert.True(doc.ContainsText(expectedWord));
    }

    [Theory]
    [InlineData("Weight: 66 kg", 66)]
    [InlineData("Height: 65 m", 65)]
    [InlineData("Height: m", -1)]
    public void TestIntegerPart(string text, int expectedValue)
    {
        Assert.Equal(expectedValue, text.GetIntPart());
    }
    
    [Theory]
    [InlineData("Weight: 66 kg", 66)]
    [InlineData("Height: 65 m", 65)]
    [InlineData("Height: 1.65 m", 1.65)]
    [InlineData("Height: m", -1)]
    public void TestDecimalPart(string text, decimal expectedDouble)
    {
        var expectedValue = (decimal?)expectedDouble;
        Assert.Equal(expectedValue, text.GetDecimalPart());
    }
    
    [Fact]
    public async Task TestLineParsingAsync()
    {
        var web = new HtmlWeb();
        var riderUrl = "https://www.procyclingstats.com/rider/sam-welsford";
        var doc = await web.LoadFromWebAsync(riderUrl);
        var infoNode = doc.DocumentNode.SelectSingleNode("//div[@class='rdr-info-cont']");
        var lines = infoNode.ParseLines();
        Assert.NotNull(lines);
        Assert.True(lines.Any());
    }
    

    [Fact]
    public async Task TestTableParsingAsync()
    {
        // Get to the table node
        var page = new HtmlWeb();
        var url = "https://www.worldcyclingstats.com/en/game/tour-down-under/2025/statistics/most-points";
        var doc = await page.LoadFromWebAsync(url);
        var resultDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='general']");
        var resultTableNode = resultDivNode.SelectSingleNode("table");

        // Retrieve the points
        var points = resultTableNode.ParseTable((row, position) =>
        {
            var name = row["Rider"]?.GetInnerText();
            var points = row["Pts"]?.GetInnerText();
            return new { Name = name, Points = points };
        }, [3]);
        
        // Check the results
        Assert.NotNull(points);
        Assert.True(points.Any());
        Assert.NotEmpty(points.First().Name);
    }
    
    [Fact]
    public async Task TestTbodyTableParsingAsync()
    {
        // Get to the table node
        var page = new HtmlWeb();
        var url = "https://www.procyclingstats.com/race/4-jours-de-dunkerque/2025";
        var doc = await page.LoadFromWebAsync(url);
        var stageRaceNode = doc.DocumentNode.SelectSingleNode("//h3[text()='Stages']");
        var stageTable = stageRaceNode.SelectSingleNode("..//table");
        // Retrieve the points
        var points = stageTable.ParseTable((row, position) =>
        {
            var date = row["Date"]?.GetInnerText();
            var stage = row["Stage"]?.GetInnerText();
            return new { Date = date, Stage = stage };
        }, [], thHeader:true);
        
        // Check the results
        Assert.NotNull(points);
        Assert.True(points.Any());
        Assert.Equal(6, points.Count);
        Assert.NotEmpty(points.First().Stage);
    }
    
    [Theory]
    [InlineData("16/05", true)]
    [InlineData("16/05/2025", true)]
    public void TestDateParsing(string input, bool shouldBeValid)
    {
        if (shouldBeValid) Assert.NotNull(input.ParseDateFromText());
        else Assert.Null(input.ParseDateFromText());
    }
}