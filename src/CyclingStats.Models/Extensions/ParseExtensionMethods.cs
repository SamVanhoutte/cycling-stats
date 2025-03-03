using System.Globalization;

namespace CyclingStats.Models.Extensions;

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

    public static decimal? ParsePercentage(this string? text)
    {
        return text?.Replace("%", "").ParseDecimalValue();
    }


    public static decimal? ParseDecimalValue(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (!decimal.TryParse(text, NumberStyles.Any, new NumberFormatInfo { NumberDecimalSeparator = "." },
                out var result))
        {
            return null;
        }

        return result;
    }

    public static bool IsNumericValue(this string text)
    {
        return int.TryParse(text, out _);
    }

    public static int? GetYearValueFromRaceId(this string? raceId)
    {
        int? year = null;
        if (string.IsNullOrWhiteSpace(raceId)) return year;
        var yearValue = raceId.Split('/').LastOrDefault(segment => segment.IsNumericValue());
        if (string.IsNullOrWhiteSpace(yearValue)) return year;
        return int.Parse(yearValue);
    }
    
    public static string? ChangeYear(this string? raceId, int year)
    {
        var raceYear = raceId.GetYearValueFromRaceId();
        if (raceYear == null) return null;
        return raceId!.Replace(raceYear.ToString()!, year.ToString());
    }

    public static DateTime? ParseDateFromText(this string? dateText, int? year = null)
    {
        year ??= DateTime.UtcNow.Year;
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

        if (dateText.Count(c => c == '/') == 1)
        {
            // The format will be day / month, so we add the year
            return new DateTime(year!.Value, int.Parse(dateText.Split('/')[1]),
                int.Parse(dateText.Split('/')[0]));
        }

        return DateTime.Parse(dateText);
    }

    public static int ParseTimeDelay(this string? timeDelay)
    {
        if (string.IsNullOrWhiteSpace(timeDelay)) return -1;
        timeDelay = timeDelay.Replace("+", "");
        timeDelay = timeDelay.Replace(" ", "");
        timeDelay = timeDelay.TrimStart().TrimEnd();

        if (timeDelay.Contains("."))
        {
            timeDelay = timeDelay.Replace(".", "'") + "''";
        }
        if (TimeSpan.TryParse(timeDelay, out var timeSpan))
        {
            return (int)timeSpan.TotalSeconds;
        }
        else
        {
            int hours = 0;
            if (timeDelay.Contains('h', StringComparison.InvariantCultureIgnoreCase))
            {
                hours = int.Parse(timeDelay.Split("h")[0]);
                timeDelay = timeDelay.Split("h")[1];
            }

            var minutes = int.Parse(timeDelay.Split("'")[0]);
            var seconds = int.Parse((timeDelay.Split("'")[1]).Replace("'", ""));
            return hours * 3600 + minutes * 60 + seconds;
        }
    }


    public static int GetIntSetting(this IDictionary<string, string>? settings, string key, int defaultValue)
    {
        var result = defaultValue;
        if (settings?.TryGetValue(key, out var val) ?? false)
        {
            if (int.TryParse(val, out var intValue))
            {
                return intValue;
            }
        }

        return defaultValue;
    }

    public static string? GetSetting(this IDictionary<string, string>? settings, string key,
        string? defaultValue = null)
    {
        var result = defaultValue;
        settings?.TryGetValue(key, out result);
        return result;
    }

    public static int? GetIntPart(this string? text, char separator = ' ')
    {
        // The format is : Date of birth: 21st September 1998 (26)
        var parts = text.Split(separator);
        var numericPart = parts.FirstOrDefault(p => int.TryParse(p, out _));
        return string.IsNullOrEmpty(numericPart) ? -1 : int.Parse(numericPart);
    }

    public static decimal? GetDecimalPart(this string? text, char separator = ' ')
    {
        // The format is : Date of birth: 21st September 1998 (26)
        var parts = text.Split(separator);
        var numericPart = parts.FirstOrDefault(p => p.ParseDecimalValue() != null);
        return string.IsNullOrEmpty(numericPart) ? -1 : numericPart.ParseDecimalValue();
    }

    public static string? ToApiNotation(this string? id)
    {
        return id?.Replace("/", "---");
    }

    public static string? FromApiNotation(this string? apiNotation)
    {
        return apiNotation?.Replace("---", "/");
    }
}