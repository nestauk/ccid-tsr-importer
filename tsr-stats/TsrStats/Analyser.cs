using System.Reflection.Metadata;
using System.Text.Json;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Document = Amazon.DynamoDBv2.DocumentModel.Document;

public class Analyser
{
    public static Dictionary<string, object> AnalyseS3Sessions(IEnumerable<S3Session> sessions)
    {
        var data = new Dictionary<string, object>
        {
            { "S3 sessions", sessions.Count() },
            { "Total participants", sessions.Sum(s => s.participants ?? 0) },
            { "Unique councils", sessions.Select(s => s.council).Distinct().Count() },
            { "Unique age ranges", string.Join(", ", sessions.SelectMany(s => JsonSerializer.Deserialize<string[]>(s.unique_age_ranges!)!).Distinct().Select(s => $"\"{s}\"") )},
            { "Unique ethnicities", string.Join(", ", sessions.SelectMany(s => JsonSerializer.Deserialize<string[]>(s.unique_ethnicities!)!).Distinct().Select(s => $"\"{s}\"") )},
            { "Unique genders", string.Join(", ", sessions.SelectMany(s => JsonSerializer.Deserialize<string[]>(s.unique_genders!)!).Distinct().Select(s => $"\"{s}\"") )}
        };

        // foreach (var council in sessions.Select(s => s.council).Distinct())
        // {
        //     var council_sessions = sessions.Where(s => s.council == council);
        //     var council_dates = council_sessions.Select(cs => cs.date);
        //     var council_path_keys = council_sessions.Select(cs => cs.path_key!);
        //     data.Add($"{council} session dates", string.Join(", ", council_dates));
        //     data.Add($"{council} session keys", string.Join(", ", council_path_keys));
        // }

        return data;
    }

    public static Dictionary<string, object> AnalyseSessions(IEnumerable<Session> sessions)
    {
        var data = new Dictionary<string, object>();
        var participants = sessions.Sum(s => s.participants!.Count);
        foreach (var session in sessions)
        {
            session.date = Utilities.UnixTimeStampToDate(session.datetime!.Value);
        }
        data.Add("Sessions", sessions.Count());
        data.Add("Total participants", participants);

        var unique_age_ranges = new HashSet<string>();
        var unique_ethnicities = new HashSet<string>();
        var unique_genders = new HashSet<string>();

        foreach (var session in sessions)
        {
            foreach (var participant in session.participants ?? new List<Document>())
            {
                if (participant.ContainsKey("demographics"))
                {
                    var demographics = participant["demographics"].AsDocument();
                    if (demographics != null)
                    {
                        var age_range = demographics["age_range"].AsString();
                        var ethnicity = demographics["ethnicity"].AsString();
                        var gender = demographics["gender"].AsString();
                        if (!string.IsNullOrWhiteSpace(age_range)) { unique_age_ranges.Add(age_range); }
                        if (!string.IsNullOrWhiteSpace(ethnicity)) { unique_ethnicities.Add(ethnicity); }
                        if (!string.IsNullOrWhiteSpace(gender)) { unique_genders.Add(gender); }
                    }
                }
                else
                {
                    throw new JsonException(session.modules!.ToJson());
                }
            }
        }
        data.Add("Unique age ranges", string.Join(", ", unique_age_ranges.Select(s => $"\"{s}\"")));
        data.Add("Unique ethnicities", string.Join(", ", unique_ethnicities.Select(s => $"\"{s}\"")));
        data.Add("Unique genders", string.Join(", ", unique_genders.Select(s => $"\"{s}\"")));

        return data;
    }

    /*
    [Index(7)] public string? unique_age_ranges { get; set; }
    [Index(8)] public string? unique_ethnicities { get; set; }
    [Index(9)] public string? unique_genders { get; set; }
    */

    public static Dictionary<string, object> AnalyseSummaries(IEnumerable<Summary> summaries)
    {
        var data = new Dictionary<string, object>();
        var summaryPopulations = new Dictionary<string, int>();
        foreach (var summary in summaries)
        {
            summaryPopulations.Add(summary.demographic!, (int)summary.participants!);
        }

        var totalPopulation = summaries.Sum(s => s.participants);
        data.Add("Demographic summaries", summaries.Count());
        data.Add("Total participants", totalPopulation!);

        // data.Add("Unique demographics", string.Join(", ", summaryPopulations.Keys.Select(s => $"\"{s}\"")));

        var unique_ages = summaryPopulations.Keys
            .SelectMany(d => d.Split(':'))
            .Where(dd => dd.Split('=')[0] == "age")
            .Select(dd => dd.Split('=')[1])
            .Distinct();

        var unique_ethnicities = summaryPopulations.Keys
            .SelectMany(d => d.Split(':'))
            .Where(dd => dd.Split('=')[0] == "ethnicity")
            .Select(dd => dd.Split('=')[1])
            .Distinct();

        var unique_genders = summaryPopulations.Keys
            .SelectMany(d => d.Split(':'))
            .Where(dd => dd.Split('=')[0] == "gender")
            .Select(dd => dd.Split('=')[1])
            .Distinct();

        data.Add("Unique age ranges", string.Join(", ", unique_ages.Select(s => $"\"{s}\"")));
        data.Add("Unique ethnicities", string.Join(", ", unique_ethnicities.Select(s => $"\"{s}\"")));
        data.Add("Unique genders", string.Join(", ", unique_genders.Select(s => $"\"{s}\"")));

        return data;
    }


}