using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO.Compression;
using CsvHelper;
using System.Globalization;

internal class Program

{
    private static string? SESSION_TABLE_NAME;
    private static string? SUMMARY_TABLE_NAME;
    private static string? S3_BUCKET_NAME;

    private static async Task Main(string[] args)
    {
        if (args.Length < 3) { Console.WriteLine("Arguments: <session-table-name> <summary-table-name>"); return; }

        S3_BUCKET_NAME = args[0];
        SESSION_TABLE_NAME = args[1];
        SUMMARY_TABLE_NAME = args[2];
        Console.WriteLine($"S3 bucket:     {S3_BUCKET_NAME}");
        Console.WriteLine($"Session table: {SESSION_TABLE_NAME}");
        Console.WriteLine($"Summary table: {SUMMARY_TABLE_NAME}");
        Console.WriteLine();

        var client = new AmazonDynamoDBClient(RegionEndpoint.EUWest2);
        var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
        var context = new DynamoDBContext(client, config);

        Console.WriteLine("Analysing sessions in S3...");
        var s3 = new AmazonS3Client(RegionEndpoint.EUWest2);
        var s3sessions = await ScanS3Async(s3, S3_BUCKET_NAME, "syndicateos-data/nesta/");
        var s3sessionInsights = AnalyseS3Sessions(s3sessions);
        PrintInsights(s3sessionInsights);

        Console.WriteLine("Analysing imported sessions...");
        var sessions = await ScanAllAsync<Session>(context, SESSION_TABLE_NAME);
        var sessionInsights = AnalyseSessions(sessions);
        PrintInsights(sessionInsights);

        Console.WriteLine("Analysing summaries...");
        var summaries = await ScanAllAsync<Summary>(context, SUMMARY_TABLE_NAME);
        var summaryInsights = AnalyseSummaries(summaries);
        PrintInsights(summaryInsights);

        Console.WriteLine("Storing analysis...");
        SaveCSV(s3sessions, "output/s3sessions.csv");
        SaveDictCSV(s3sessionInsights, "output/s3session_insights.csv");
        SaveCSV(sessions, "output/sessions.csv");
        SaveDictCSV(sessionInsights, "output/session_insights.csv");
        SaveCSV(summaries, "output/summaries.csv");
        SaveDictCSV(summaryInsights, "output/summary_insights.csv");
        Console.WriteLine("Done.");
        Console.WriteLine();
    }

    private static void SaveDictCSV(IDictionary<string, object> data, string path)
    {
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data.ToList().Select(kvp => new { insight = kvp.Key, value = kvp.Value.ToString() }));
        }
    }

    private static void SaveCSV<T>(IEnumerable<T> data, string path)
    {
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords<T>(data);
        }
    }

    private static void PrintInsights(Dictionary<string, object> insights)
    {
        Console.WriteLine(string.Join('\n', insights.Select(s => $"{s.Key}: {s.Value}")));
        Console.WriteLine();
    }

    private static Dictionary<string, object> AnalyseSessions(IEnumerable<Session> sessions)
    {
        var data = new Dictionary<string, object>();
        var participants = sessions.Sum(s => s.participants!.Count);
        foreach (var session in sessions)
        {
            session.date = UnixTimeStampToDate(session.datetime!.Value);
        }
        data.Add("Sessions", sessions.Count());
        data.Add("Total participants", participants);
        return data;
    }

    private static Dictionary<string, object> AnalyseSummaries(IEnumerable<Summary> summaries)
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

    private static async Task<IEnumerable<T>> ScanAllAsync<T>(DynamoDBContext context, string table)
    {
        var items = new List<T>();
        var config = new DynamoDBOperationConfig { OverrideTableName = table };
        var search = context.ScanAsync<T>(null, config);
        bool finished;
        do
        {
            items.AddRange(await search.GetNextSetAsync());
            finished = search.IsDone;
        } while (!finished);
        return items;
    }

    private static async Task<IEnumerable<S3Session>> ScanS3Async(AmazonS3Client client, string bucket, string prefix)
    {
        var paginator = client.Paginators.ListObjectsV2(new ListObjectsV2Request
        {
            BucketName = bucket,
            Prefix = prefix
        });

        var entries = new List<Tuple<string, long>>();
        await foreach (var response in paginator.Responses)
        {
            foreach (var entry in response.S3Objects)
            {
                entries.Add(new Tuple<string, long>(entry.Key, entry.Size));
            }
        }

        var path_keys = entries
            .Where(e => e.Item1.StartsWith(prefix))
            .Where(e => e.Item1.EndsWith(".zip"))
            .Select(e => e.Item1[prefix.Length..].Split('/')[0]).Distinct();

        var sessions = new List<S3Session>();
        var errors = new Dictionary<string, List<string>>();
        foreach (var path_key in path_keys)
        {
            var largest_file_path = entries.Where(e => e.Item1.Contains($"/{path_key}/")).OrderByDescending(e => e.Item2).First();
            using (var stream = await GetS3StreamAsync(client, bucket, largest_file_path.Item1))
            {
                try
                {
                    using var zip = new ZipArchive(stream);
                    var votes_entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith("stage_slider_vote_votes.csv"));
                    var text_inputs_entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith("stage_text_input_votes.csv"));
                    var timings_entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith("stage_timings.csv"));

                    if (votes_entry == null) throw new FileNotFoundException($"{largest_file_path} does not contain stage_slider_vote_votes.csv");
                    if (timings_entry == null) throw new FileNotFoundException($"{largest_file_path} does not contain stage_timings.csv");
                    if (text_inputs_entry == null) throw new FileNotFoundException($"{largest_file_path} does not contain stage_text_input_votes.csv");

                    var slider_vote_votes = ReadCSV<StageSliderVoteVotes>(votes_entry.Open());
                    var text_inputs = ReadCSV<StageTextInputVotes>(text_inputs_entry.Open());
                    var timings = ReadCSV<StageTimings>(timings_entry.Open());

                    var council = text_inputs.First(ti => ti.stage_id == "local-authority").vote;
                    var session_id = text_inputs.First(ti => ti.stage_id == "local-authority").session_id;
                    var any_timestamp = timings.First().end_time;
                    var date = UnixTimeStampToDate(any_timestamp);
                    var participants = slider_vote_votes.Select(r => r.cast_uuid).Distinct();

                    sessions.Add(new S3Session
                    {
                        path_key = path_key.Trim(),
                        participants = participants.Count(),
                        council = council!.Trim(),
                        date = date.Trim(),
                        session_id = session_id!.Trim()
                    });
                    Console.Write('.');
                }
                catch (Exception e)
                {
                    if (!errors.ContainsKey(path_key)) errors.Add(path_key, new List<string>());
                    errors[path_key].Add($"{e.GetType().Name}: {e.Message}");
                }
            } // memory stream
        }
        Console.WriteLine();
        Console.WriteLine($"{errors.Count} errors");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error.Key}: {string.Join(", ", error.Value)}");
        }

        Console.WriteLine();
        return sessions;
    }

    private static string UnixTimeStampToDate(long timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime().Date.ToString("yyyy-MM-dd");

    private static IEnumerable<T> ReadCSV<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
        {
            return csvReader.GetRecords<T>().ToList();
        }
    }

    private static async Task<Stream> GetS3StreamAsync(AmazonS3Client client, string bucket, string key)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key,
        };
        var memoryStream = new MemoryStream();
        using var response = await client.GetObjectAsync(request);
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream;
    }

    private static Dictionary<string, object> AnalyseS3Sessions(IEnumerable<S3Session> sessions)
    {
        var data = new Dictionary<string, object>();
        data.Add("S3 sessions", sessions.Count());
        data.Add("Total participants", sessions.Sum(s => s.participants));
        data.Add("Unique councils", sessions.Select(s => s.council).Distinct().Count());
        foreach (var council in sessions.Select(s => s.council).Distinct())
        {
            var council_sessions = sessions.Where(s => s.council == council);
            var council_dates = council_sessions.Select(cs => cs.date);
            var council_path_keys = council_sessions.Select(cs => cs.path_key!);
            // data.Add($"{council} session dates", string.Join(", ", council_dates));
            // data.Add($"{council} session keys", string.Join(", ", council_path_keys));
        }

        return data;
    }

}

