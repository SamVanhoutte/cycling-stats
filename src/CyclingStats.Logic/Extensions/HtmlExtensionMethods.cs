using HtmlAgilityPack;

namespace CyclingStats.Logic;

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
}