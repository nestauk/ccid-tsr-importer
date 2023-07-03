using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;

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

    private static async Task<IEnumerable<S3Session>?> ScanS3Async(AmazonS3Client client, string bucket, string prefix)
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
        foreach (var path_key in path_keys)
        {
            var largest = entries.Where(e => e.Item1.Contains($"/{path_key}/")).OrderByDescending(e => e.Item2).First();
            sessions.Add(new S3Session
            {
                path_key = path_key,
            });
        }

        return sessions;
    }

    private static Dictionary<string, object> AnalyseS3Sessions(IEnumerable<S3Session> sessions)
    {
        var data = new Dictionary<string, object>();
        data.Add("S3 sessions", sessions.Count());
        return data;
    }

}

