using System.Reflection.Metadata;
using System.Text.Json;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Document = Amazon.DynamoDBv2.DocumentModel.Document;
using System.Linq;

public class Analyser
{
    public static Dictionary<string, object> AnalyseS3Sessions(IEnumerable<S3Session> sessions)
    {
        var data = new Dictionary<string, object>
        {
            { "S3 sessions", sessions.Count() },
            { "Total participants", sessions.Sum(s => s.participants ?? 0) },
            { "Unique councils", sessions.Select(s => s.council).Distinct().Count() },
            { "Unique age ranges", string.Join(", ", sessions.SelectMany(s => JsonSerializer.Deserialize<string[]>(s.unique_age_ranges!)!).Distinct().Select(s => $"\"{s}\"").OrderBy(s => s))},
            { "Unique ethnicities", string.Join(", ", sessions.SelectMany(s => JsonSerializer.Deserialize<string[]>(s.unique_ethnicities!)!).Distinct().Select(s => $"\"{s}\"").OrderBy(s => s))},
            { "Unique genders", string.Join(", ", sessions.SelectMany(s => JsonSerializer.Deserialize<string[]>(s.unique_genders!)!).Distinct().Select(s => $"\"{s}\"").OrderBy(s => s))}
        };

        return data;
    }

    public static Tuple<HashSet<string>, HashSet<string>, HashSet<string>> GetUniqueDemographics_Age_Ethnicity_Gender(IEnumerable<Session> sessions)
    {
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
                        var age_range = demographics["age_range"] is DynamoDBNull ? null : demographics["age_range"].AsString();
                        var ethnicity = demographics["ethnicity"] is DynamoDBNull ? null : demographics["ethnicity"].AsString();
                        var gender = demographics["gender"] is DynamoDBNull ? null : demographics["gender"].AsString();
                        if (!string.IsNullOrWhiteSpace(age_range)) { unique_age_ranges.Add(age_range); }
                        if (!string.IsNullOrWhiteSpace(ethnicity)) { unique_ethnicities.Add(ethnicity); }
                        if (!string.IsNullOrWhiteSpace(gender)) { unique_genders.Add(gender); }
                    }
                }
                else
                {
                    throw new JsonException($"Participant found without demographics in:\n{session.modules!.ToJson()}");
                }
            }
        }
        return new Tuple<HashSet<string>, HashSet<string>, HashSet<string>>(unique_age_ranges, unique_ethnicities, unique_genders);

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

        var unique_demographics = GetUniqueDemographics_Age_Ethnicity_Gender(sessions);

        data.Add("Unique age ranges", string.Join(", ", unique_demographics.Item1.Select(s => $"\"{s}\"").OrderBy(s => s)));
        data.Add("Unique ethnicities", string.Join(", ", unique_demographics.Item2.Select(s => $"\"{s}\"").OrderBy(s => s)));
        data.Add("Unique genders", string.Join(", ", unique_demographics.Item3.Select(s => $"\"{s}\"").OrderBy(s => s)));

        return data;
    }

    public static string NormaliseDemographic(string? council = null, string? age_range = null, string? ethnicity = null, string? gender = null)
    {
        var council_norm = Utilities.NormaliseDemographic(council);
        var age_range_norm = Utilities.NormaliseDemographic(age_range);
        var ethnicity_norm = Utilities.NormaliseDemographic(ethnicity);
        var gender_norm = Utilities.NormaliseDemographic(gender);
        if (string.IsNullOrWhiteSpace(council_norm))
        {
            return $"age_range={age_range_norm ?? "*"}:ethnicity={ethnicity_norm ?? "*"}:gender={gender_norm ?? "*"}";
        }
        else
        {
            return $"council={council_norm ?? "*"}:age_range={age_range_norm ?? "*"}:ethnicity={ethnicity_norm ?? "*"}:gender={gender_norm ?? "*"}";
        }
    }

    public static string DescribeDemographic(string? council = null, string? age_range = null, string? ethnicity = null, string? gender = null)
    {
        var parts = new List<string>();
        if (council != null) { parts.Add("Council: " + $"'{council}'"); }
        if (age_range != null) { parts.Add("Age range: " + $"'{age_range}'"); }
        if (ethnicity != null) { parts.Add("Ethnicity: " + $"'{ethnicity}'"); }
        if (gender != null) { parts.Add("Gender: " + $"'{gender}'"); }
        if (age_range == null && ethnicity == null && gender == null) { parts.Add("All participants"); }
        return string.Join(", ", parts);
    }

    private static void IncrementDemographic(Dictionary<string, int> counts, string demographic)
    {
        if (!counts.ContainsKey(demographic)) { throw new Exception("Demographic not found: " + demographic); }
        counts[demographic] += 1;
    }

    public static Dictionary<string,string> CalculateUniqueDemographicCombinations(IEnumerable<string> all_age_ranges, IEnumerable<string> all_ethnicities, IEnumerable<string> all_genders)
    {
        var uniques = new Dictionary<string,string>();

        uniques.Add(NormaliseDemographic(null, null, null, null), DescribeDemographic(null, null, null, null));

        foreach (var gender in all_genders)
        {
            if (AllNotEmpty(gender)) { uniques.Add(NormaliseDemographic(null, null, null, gender), DescribeDemographic(null, null, null, gender)); }
        }
        foreach (var age_range in all_age_ranges)
        {
            if (AllNotEmpty(age_range)) { uniques.Add(NormaliseDemographic(null, age_range, null, null), DescribeDemographic(null, age_range, null, null)); }
        }
        foreach (var ethnicity in all_ethnicities)
        {
            if (AllNotEmpty(ethnicity)) { uniques.Add(NormaliseDemographic(null, null, ethnicity, null), DescribeDemographic(null, null, ethnicity, null)); }
        }

        foreach (var gender in all_genders.Where(g => !string.IsNullOrWhiteSpace(g)))
        {
            foreach (var age_range in all_age_ranges)
            {
                foreach (var ethnicity in all_ethnicities)
                {
                    if (!uniques.ContainsKey(NormaliseDemographic(null, age_range, ethnicity, null))) { uniques.Add(NormaliseDemographic(null, age_range, ethnicity, null), DescribeDemographic(null, age_range, ethnicity, null)); }
                    if (!uniques.ContainsKey(NormaliseDemographic(null, age_range, null, gender))) { uniques.Add(NormaliseDemographic(null, age_range, null, gender), DescribeDemographic(null, age_range, null, gender)); }
                    if (!uniques.ContainsKey(NormaliseDemographic(null, null, ethnicity, gender))) { uniques.Add(NormaliseDemographic(null, null, ethnicity, gender), DescribeDemographic(null, null, ethnicity, gender)); }
                } // all_ethnicities
            } // all_age_ranges
        } // all_genders

        // the very specific triple combos
        foreach (var gender in all_genders)
        {
            foreach (var age_range in all_age_ranges)
            {
                foreach (var ethnicity in all_ethnicities)
                {
                    if (AllNotEmpty(gender, age_range, ethnicity)) { uniques.Add(NormaliseDemographic(null, age_range, ethnicity, gender), DescribeDemographic(null, age_range, ethnicity, gender)); }
                } // all_ethnicities
            } // all_age_ranges
        } // all_genders

        return uniques;
    }

    public static IEnumerable<IEnumerable<object>> AnalyseSessionDemographics(IEnumerable<Session> sessions, Dictionary<string, object> sessionsAnalysis)
    {
        var unique_demographics = GetUniqueDemographics_Age_Ethnicity_Gender(sessions);
        var all_age_ranges = unique_demographics.Item1;
        var all_ethnicities = unique_demographics.Item2;
        var all_genders = unique_demographics.Item3;
        var unique_demographic_combos = CalculateUniqueDemographicCombinations(all_age_ranges, all_ethnicities, all_genders);
        var unique_demographic_strings = unique_demographic_combos.Keys;

        var data = new List<Dictionary<string, object>>();

        foreach (var session in sessions.OrderBy(s => s.datetime!.Value))
        {
            var session_id = session.sessionId;
            var session_council_text = session.council;
            var session_council = NormaliseDemographic(session_council_text);
            var session_datestamp = session.datetime!.Value;
            var session_date = Utilities.UnixTimeStampToDate(session.datetime!.Value);

            var demographic_counts = unique_demographic_strings.ToDictionary(d => d, d => 0);

            foreach (var participant in session.participants ?? new List<Document>())
            {
                if (participant.ContainsKey("demographics"))
                {
                    IncrementDemographic(demographic_counts, NormaliseDemographic(null, null, null, null)); // the "all participants" count
                    var demographics = participant["demographics"].AsDocument();
                    if (demographics != null)
                    {
                        var age_range = demographics["age_range"] is DynamoDBNull ? null : demographics["age_range"].AsString();
                        var ethnicity = demographics["ethnicity"] is DynamoDBNull ? null : demographics["ethnicity"].AsString();
                        var gender = demographics["gender"] is DynamoDBNull ? null : demographics["gender"].AsString();

                        if (AllNotEmpty(age_range)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, age_range, null, null)); }
                        if (AllNotEmpty(ethnicity)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, null, ethnicity, null)); }
                        if (AllNotEmpty(gender)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, null, null, gender)); }
                        if (AllNotEmpty(age_range, ethnicity)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, age_range, ethnicity, null)); }
                        if (AllNotEmpty(age_range, gender)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, age_range, null, gender)); }
                        if (AllNotEmpty(ethnicity, gender)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, null, ethnicity, gender)); }
                        if (AllNotEmpty(age_range, ethnicity, gender)) { IncrementDemographic(demographic_counts, NormaliseDemographic(null, age_range, ethnicity, gender)); }
                    }
                }
                else
                {
                    throw new JsonException($"Participant found without demographics in:\n{session.modules!.ToJson()}");
                }
            } // session.participants

            var session_summary = new Dictionary<string, object>
            {
                { "heading", "session: " + (data.Count() + 1).ToString("D3") },
                { "session_id", session_id },
                { "session_council", session_council_text },
                { "session_datestamp", session_datestamp },
                { "session_date", session_date }
            };

            foreach (var demographic_kvp in demographic_counts)
            {
                session_summary.Add(demographic_kvp.Key, demographic_kvp.Value);
            }

            data.Add(session_summary);
        } // sessions

        var output_headers = new List<object>();
        output_headers.Add("statistic");
        output_headers.Add("description");
        output_headers.AddRange(data.OrderBy(d => d["session_datestamp"]).Select(d => d["heading"]));

        var output_rows = new List<List<object>>();
        var sid_row = new List<object> { "session_id", "A unique id for each workshop, assigned by Syndicate" };
        sid_row.AddRange(output_headers.Skip(2).Select(h => data.Single(d => d["heading"] == h)["session_id"]));
        var council_row = new List<object> { "council", "The local authority where the workshop took place" };
        council_row.AddRange(output_headers.Skip(2).Select(h => data.Single(d => d["heading"] == h)["session_council"]));
        var datestamp_row = new List<object> { "datestamp", "A unix timestamp indicating the start of the session" };
        datestamp_row.AddRange(output_headers.Skip(2).Select(h => data.Single(d => d["heading"] == h)["session_datestamp"]));
        var date_row = new List<object> { "date", "The date the session took place" };
        date_row.AddRange(output_headers.Skip(2).Select(h => data.Single(d => d["heading"] == h)["session_date"]));

        output_rows.Add(output_headers);
        output_rows.Add(sid_row);
        output_rows.Add(council_row);
        output_rows.Add(datestamp_row);
        output_rows.Add(date_row);

        foreach (var demo in unique_demographic_combos)
        {
            var demo_row = new List<object>() { demo.Key, demo.Value };
            demo_row.AddRange(output_headers.Skip(2).Select(h => data.Single(d => d["heading"] == h)[demo.Key]));
            output_rows.Add(demo_row);
        }

        // data.OrderBy(d => d["session_datestamp"]);
        return output_rows;
    }

    public static bool AllNotEmpty(params string?[] strings)
    {
        return strings.All(s => !string.IsNullOrWhiteSpace(s));
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

        var unique_councils = summaryPopulations.Keys
            .SelectMany(d => d.Split(':'))
            .Where(dd => dd.Split('=')[0] == "council")
            .Select(dd => dd.Split('=')[1])
            .Distinct();

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

        data.Add("Unique councils", string.Join(", ", unique_councils.Select(s => $"\"{s}\"").OrderBy(s => s)));
        data.Add("Unique age ranges", string.Join(", ", unique_ages.Select(s => $"\"{s}\"").OrderBy(s => s)));
        data.Add("Unique ethnicities", string.Join(", ", unique_ethnicities.Select(s => $"\"{s}\"").OrderBy(s => s)));
        data.Add("Unique genders", string.Join(", ", unique_genders.Select(s => $"\"{s}\"").OrderBy(s => s)));

        return data;
    }

    public static Dictionary<string, object> AnalyseEndpointData(JsonDocument data)
    {
        var insights = new Dictionary<string, object>();

        insights.Add("Participants", data.RootElement.GetProperty("summary").GetProperty("participants").GetInt32());

        var councils = data.RootElement.GetProperty("all_demographics").GetProperty("councils").EnumerateArray().Select(council => council.GetString());
        var ages = data.RootElement.GetProperty("all_demographics").GetProperty("ages").EnumerateArray().Select(age => age.GetString());
        var ethnicities = data.RootElement.GetProperty("all_demographics").GetProperty("ethnicities").EnumerateArray().Select(ethnicity => ethnicity.GetString());
        var genders = data.RootElement.GetProperty("all_demographics").GetProperty("genders").EnumerateArray().Select(gender => gender.GetString());

        insights.Add("Councils", string.Join(", ", councils.Select(s => $"\"{s}\"").OrderBy(s => s)));
        insights.Add("Age ranges", string.Join(", ", ages.Select(s => $"\"{s}\"").OrderBy(s => s)));
        insights.Add("Ethnicities", string.Join(", ", ethnicities.Select(s => $"\"{s}\"").OrderBy(s => s)));
        insights.Add("Genders", string.Join(", ", genders.Select(s => $"\"{s}\"").OrderBy(s => s)));

        return insights;
    }

}