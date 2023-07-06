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
using System.Text.Json;

internal class Program

{
    private static string? SESSION_TABLE_NAME;
    private static string? SUMMARY_TABLE_NAME;
    private static string? S3_BUCKET_NAME;

    private static async Task Main(string[] args)
    {
        if (args.Length < 3) { Console.WriteLine("Arguments: <s3-bucket-name> <session-table-name> <summary-table-name>"); return; }

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
        var s3sessions = await S3Scanner.ScanS3Async(s3, S3_BUCKET_NAME, "syndicateos-data/nesta/");
        var s3sessionInsights = Analyser.AnalyseS3Sessions(s3sessions);
        PrintInsights(s3sessionInsights);

        Console.WriteLine("Analysing imported sessions...");
        var sessions = await DynamoScanner.ScanAllAsync<Session>(context, SESSION_TABLE_NAME);
        var sessionInsights = Analyser.AnalyseSessions(sessions);
        PrintInsights(sessionInsights);

        Console.WriteLine("Analysing summaries...");
        var summaries = await DynamoScanner.ScanAllAsync<Summary>(context, SUMMARY_TABLE_NAME);
        var summaryInsights = Analyser.AnalyseSummaries(summaries);
        PrintInsights(summaryInsights);

        Console.WriteLine("Storing analysis...");
        CsvUtilities.SaveCSV(s3sessions, "output/s3sessions.csv");
        CsvUtilities.SaveDictCSV(s3sessionInsights, "output/s3session_insights.csv");
        CsvUtilities.SaveCSV(sessions, "output/sessions.csv");
        CsvUtilities.SaveDictCSV(sessionInsights, "output/session_insights.csv");
        CsvUtilities.SaveCSV(summaries, "output/summaries.csv");
        CsvUtilities.SaveDictCSV(summaryInsights, "output/summary_insights.csv");
        Console.WriteLine("Done.");
        Console.WriteLine();
    }

    private static void PrintInsights(Dictionary<string, object> insights)
    {
        Console.WriteLine(string.Join('\n', insights.Select(s => $"{s.Key}: {s.Value}")));
        Console.WriteLine();
    }
}

