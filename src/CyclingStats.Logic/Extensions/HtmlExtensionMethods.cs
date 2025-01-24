using System.Text;
using HtmlAgilityPack;

namespace CyclingStats.Logic.Extensions;

public static class HtmlExtensionMethods
{
    public static string? GetInnerText(this HtmlNode? node)
    {
        return node == null ? "" : HtmlEntity.DeEntitize(node.InnerText).TrimStart().TrimEnd();
    }

    public static string? GetAttributeText(this HtmlNode? node, string attributeName)
    {
        return node == null
            ? ""
            : HtmlEntity.DeEntitize(node.GetAttributeValue(attributeName, "")).TrimStart().TrimEnd();
    }

    public static bool ContainsText(this HtmlDocument? document, params string[] keyWords)
    {
        return document.ContainsText(StringComparison.InvariantCultureIgnoreCase, keyWords);
    }

    public static bool ContainsText(this HtmlDocument? document, StringComparison stringComparison,
        params string[] keyWords)
    {
        return keyWords.Any(keyWord => document?.DocumentNode.InnerText.Contains(keyWord, stringComparison) ?? false);
    }

    public static List<string>? ParseLines(this HtmlNode? node, string untilElement = "br")
    {
        if (node == null) return null;
        var lines = new List<string> { };
        var currentLine = new StringBuilder();
        foreach (var childNode in node.ChildNodes)
        {
            if (childNode.Name.Equals(untilElement, StringComparison.CurrentCultureIgnoreCase))
            {
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString());
                }
                currentLine = new StringBuilder();
            }
            else
            {
                currentLine.Append(childNode.InnerText);
            }
        }
        lines.Add(currentLine.ToString());
        return lines;
    }
    
    public static List<T>? ParseTable<T>(this HtmlNode? tableNode,
        Func<Dictionary<string, HtmlNode?>, int, T> rowParser,
        int[]? bodyColumnsToSkip = null, bool thHeader = false, int? top = null)
    {
        var result = new List<T>();
        var headerNames = new List<string> { };
        if (tableNode == null) return null;
        var rows = tableNode.SelectNodes(".//tr");
        // Read table column headers
        var headerRow = rows?.FirstOrDefault();
        if (headerRow == null || rows == null) return null;
        var headerIndex = 1;
        foreach (var header in headerRow.SelectNodes(thHeader ? "th" : "td"))
        {
            headerNames.Add(string.IsNullOrEmpty(header.InnerText)
                ? $"Column{headerIndex}"
                : header.InnerText.Replace("&nbsp;", "").Trim());
            headerIndex++;
        }

        // For every row, call the parser function
        var relevantRows = rows.Skip(1);
        if (top != null)
        {
            relevantRows = relevantRows.Take(top.Value);
        }

        var rowIndex = 0;
        foreach (var row in relevantRows)
        {
            var columns = row.SelectNodes("td");
            if (headerNames.Count == (columns.Count - (bodyColumnsToSkip?.Length ?? 0)))
            {
                int index = 0;
                int colIndex = 0;
                var namedColumns = new Dictionary<string, HtmlNode?>();
                foreach (var column in columns)
                {
                    if (!(bodyColumnsToSkip?.Contains(index) ?? false))
                    {
                        namedColumns.Add(headerNames[colIndex], column);
                        colIndex++;
                    }

                    index++;
                }

                var tValue = rowParser(namedColumns, rowIndex);
                if (tValue != null)
                {
                    result.Add(tValue);
                }
                rowIndex++;
            }
        }

        return result;
    }
}