using System.Globalization;
using CyclingStats.Logic.Extensions;
using CyclingStats.Models;
using HtmlAgilityPack;

namespace WorldcyclingStats;

public class WcsStatsCollector
{



    public Task<IEnumerable<CyclingTeam>> GetTeamsAsync()
    {
        throw new NotImplementedException();
    }

    private int GetDiscipline(HtmlDocument doc, string discipline)
    {
        var disciplineDiv = doc.DocumentNode.SelectSingleNode($"//div[text()='&nbsp;{discipline}']");
        var value = disciplineDiv.GetAttributeText("aria-valuenow");
        if (string.IsNullOrEmpty(value)) return 0;
        var percentage = double.Parse(value, CultureInfo.InvariantCulture);
        return (int)(Math.Round(percentage));
    }

    


    public static List<string> TeamNames => new List<string>
    {
        "groupama-fdj", "team-arkéa-samsic", "ag2r-citroen-team", "cofidis", "ef-education---easypost",
        "team-totalenergies", "israel---premier-tech", "uno-x-pro-cycling-team", "lotto---dstny",
        "intermarché---circus---wanty", "alpecin---deceuninck", "bingoal---wb", "bora-hansgrohe",
        "tudor-pro-cycling-team", "uae-team-emirates", "ineos-grenadiers", "nice-métropole-côte-d'azur",
        "team-flanders---baloise", "trek-segafredo", "quick-step-alpha-vinyl-team", "astana-qazaqstan-team",
        "bahrain-victorious", "bardiani-csf-faizane", "jumbo-visma", "movistar-team", "team-bikeexchange-jayco",
        "team-dsm"
    };
}