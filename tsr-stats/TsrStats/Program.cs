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
using TsrStats.Entities.WorkshopSummary;

internal class Program

{
    private static string? SESSION_TABLE_NAME;
    private static string? SUMMARY_TABLE_NAME;
    private static string? INDIVIDUAL_WORKSHOP_SUMMARY_TABLE_NAME;
    private static string? S3_BUCKET_NAME;
    private static string? DATA_ENDPOINT_URL;

    private static async Task Main(string[] args)
    {
        if (args.Length < 5) { Console.WriteLine("Arguments: <s3-bucket-name> <session-table-name> <summary-table-name> <data-endpoint-url> <individual-workshop-summary-table>"); return; }

        S3_BUCKET_NAME = args[0];
        SESSION_TABLE_NAME = args[1];
        SUMMARY_TABLE_NAME = args[2];
        DATA_ENDPOINT_URL = args[3];
        INDIVIDUAL_WORKSHOP_SUMMARY_TABLE_NAME = args[4];
        Console.WriteLine($"S3 bucket:                          {S3_BUCKET_NAME}");
        Console.WriteLine($"Session table:                      {SESSION_TABLE_NAME}");
        Console.WriteLine($"Summary table:                      {SUMMARY_TABLE_NAME}");
        Console.WriteLine($"Data endpoint:                      {DATA_ENDPOINT_URL}");
        Console.WriteLine($"Individual workshops summary table: {INDIVIDUAL_WORKSHOP_SUMMARY_TABLE_NAME}");
        Console.WriteLine();

        var client = new AmazonDynamoDBClient(RegionEndpoint.EUWest2);
        var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
        var context = new DynamoDBContext(client, config);

        Console.WriteLine("Analysing sessions in S3...");
        var s3 = new AmazonS3Client(RegionEndpoint.EUWest2);
        var s3sessions = await S3Scanner.ScanS3Async(s3, S3_BUCKET_NAME, "syndicateos-data/nesta/");
        var sessionIdToKeyMap = s3sessions.Where(s => !string.IsNullOrWhiteSpace(s.session_id)).ToDictionary(s => s.session_id!, s => s.path_key!);
        var s3sessionInsights = Analyser.AnalyseS3Sessions(s3sessions);
        PrintInsights(s3sessionInsights);

        Console.WriteLine("Analysing imported sessions...");
        var sessions = await DynamoScanner.ScanAllAsync<Session>(context, SESSION_TABLE_NAME);
        var sessionInsights = Analyser.AnalyseSessions(sessions);
        PrintInsights(sessionInsights);

        Console.WriteLine("Analysing imported session demographics...");
        var sessionDemographicInsights = Analyser.AnalyseSessionDemographics(sessions, sessionInsights);        
        Console.WriteLine();

        Console.WriteLine("Analysing summaries...");
        var summaries = await DynamoScanner.ScanAllAsync<Summary>(context, SUMMARY_TABLE_NAME);
        var summaryInsights = Analyser.AnalyseSummaries(summaries);
        PrintInsights(summaryInsights);

        Console.WriteLine("Analysing individual workshop summaries...");
        var individualWorkshopDataRaw = await DynamoScanner.ScanAllAsync<WorkshopSummary>(context, INDIVIDUAL_WORKSHOP_SUMMARY_TABLE_NAME);
        var individualWorkshopData = individualWorkshopDataRaw.ToList().Select((workshop) => 
        { 
            workshop.workshopNumber = sessionIdToKeyMap[workshop.sessionId];
            return workshop;
        });
        Console.WriteLine();

        Console.WriteLine("Storing analysis...");
        FileUtilities.SaveCSV(s3sessions, "output/s3sessions.csv");
        FileUtilities.SaveDictAsInsightValueCSV(s3sessionInsights, "output/s3session_insights.csv");
        FileUtilities.SaveCSV(sessions, "output/sessions.csv");
        FileUtilities.SaveListListCSV(sessionDemographicInsights, "output/sessions_demographics.csv");
        FileUtilities.SaveDictAsInsightValueCSV(sessionInsights, "output/sessions_insights.csv");
        FileUtilities.SaveCSV(summaries, "output/summaries.csv");
        FileUtilities.SaveDictAsInsightValueCSV(summaryInsights, "output/summaries_insights.csv");
        FileUtilities.SaveAllToDirectoryByCouncil(individualWorkshopData, "output/workshops", sessionIdToKeyMap);
        Console.WriteLine();

        Console.WriteLine("Analysing endpoint...");
        var endpointData = await EndpointReader.ReadEndpointAsync(DATA_ENDPOINT_URL);
        var endpointDataInsights = Analyser.AnalyseEndpointData(endpointData);
        PrintInsights(endpointDataInsights);

        Console.WriteLine("Storing endpoint data...");
        FileUtilities.SaveJson(endpointData, "output/endpoint_data.json");
        FileUtilities.SaveDictAsInsightValueCSV(endpointDataInsights, "output/endpoint_data_insights.csv");
        Console.WriteLine();

        Console.WriteLine("Done.");
        Console.WriteLine();
    }

    private static void PrintInsights(Dictionary<string, object> insights)
    {
        Console.WriteLine(string.Join('\n', insights.Select(s => $"{s.Key}: {s.Value}")));
        Console.WriteLine();
    }
}

