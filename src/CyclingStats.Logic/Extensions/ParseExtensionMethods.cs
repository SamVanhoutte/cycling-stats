using System.Globalization;
using HtmlAgilityPack;

namespace CyclingStats.Logic;

public static class ParseExtensionMethods
{
    public static decimal? ParseDistance(this string? distanceText)
    {
        if (string.IsNullOrWhiteSpace(distanceText)) return -1;
        return decimal.Parse(distanceText.Split(" ").First(), CultureInfo.InvariantCulture);
    }
    
    public static int? ParseInteger(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (!int.TryParse(text, out var result))
        {
            return null;
        }

        return result;
    }
    
    public static DateTime? ParseDateFromText(this string? dateText)
    {
        if (string.IsNullOrWhiteSpace(dateText)) return null;
        if (dateText.Contains('(') && dateText.Contains(')'))
        {
            // The date may be formatted like this: "24 February 2024  (Saturday)", so we handle it
            dateText = dateText.Split('(')[0].Trim();
        }
        
        if (dateText.Contains('-'))
        {
            // This can be the case for stage races, so we take the first part of the date
            dateText = dateText.Split('-')[0].Trim();
        }

        return DateTime.Parse(dateText);
    }

    public static int ParseTimeDelay(this string? timeDelay)
    {
        if (string.IsNullOrWhiteSpace(timeDelay)) return -1;
        timeDelay = timeDelay.Replace("+", "");
        timeDelay = timeDelay.Replace(" ", "");
        timeDelay = timeDelay.TrimStart().TrimEnd();
        var minutes = int.Parse(timeDelay.Split("'")[0]);
        var seconds = int.Parse((timeDelay.Split("'")[1]).Replace("'", ""));
        return minutes * 60 + seconds;
    }
    

}