using System.Text.Json;

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
        return data;
    }

    public static Dictionary<string, object> AnalyseSummaries(IEnumerable<Summary> summaries)
    {
        var data = new Dictionary<string, object>();
        var summaryPopulations = new Dictionary<string, int>();
        foreach (var summary in summaries)
            summaryPopulations.Add(summary.demographic!, (int)summary.participants!);
        var totalPopulation = summaries.Sum(s => s.participants);
        data.Add("Demographic summaries", summaries.Count());
        data.Add("Total participants", totalPopulation!);
        return data;
    }


}