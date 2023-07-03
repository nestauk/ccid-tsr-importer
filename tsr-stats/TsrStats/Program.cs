using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Linq;

internal class Program
{
    private static string SESSION_TABLE_NAME;
    private static string SUMMARY_TABLE_NAME;

    private static async Task Main(string[] args)
    {
        if (args.Length < 2) { Console.WriteLine("Arguments: <session-table-name> <summary-table-name>"); return; }

        SESSION_TABLE_NAME = args[0];
        SUMMARY_TABLE_NAME = args[1];
        Console.WriteLine($"Session table: {SESSION_TABLE_NAME}");
        Console.WriteLine($"Summary table: {SUMMARY_TABLE_NAME}");
        Console.WriteLine();

        var client = new AmazonDynamoDBClient(RegionEndpoint.EUWest2);
        var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
        var context = new DynamoDBContext(client, config);

        Console.WriteLine("Analysing sessions...");
        var sessionData = await AnalyseSessions(context);
        Console.WriteLine(string.Join('\n', sessionData.Select(s => $"{s.Key}: {s.Value}")));
        Console.WriteLine();

        Console.WriteLine("Analysing summaries...");
        var summaryData = await AnalyseSummaries(context);
        Console.WriteLine(string.Join('\n', summaryData.Select(s => $"{s.Key}: {s.Value}")));
        Console.WriteLine();
    }

    //     private static async Task<IEnumerable<Session>> ReadSessions(string path)
    //     {
    //         using (var reader = new StreamReader("path\\to\\file.csv"))
    // using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    // {
    //     var records = csv.GetRecords<Foo>();
    // }
    //     }

    private static async Task<Dictionary<string, object>> AnalyseSessions(DynamoDBContext context)
    {
        var data = new Dictionary<string, object>();
        var sessions = await ScanAll<Session>(context, SESSION_TABLE_NAME);
        data.Add("Sessions", sessions.Count());

        var participants = sessions.Sum(s => s.participants!.Count);
        data.Add("Total participants", participants);

        return data;
    }

    private static async Task<Dictionary<string, object>> AnalyseSummaries(DynamoDBContext context)
    {
        var data = new Dictionary<string, object>();
        var summaries = await ScanAll<Summary>(context, SUMMARY_TABLE_NAME);
        data.Add("Summaries", summaries.Count());

        var summaryPopulations = new Dictionary<string, int>();
        foreach (var summary in summaries)
            summaryPopulations.Add(summary.demographic!, (int)summary.participants!);
        var totalPopulation = summaries.Sum(s => s.participants);
        data.Add("Total participants", totalPopulation!);

        return data;
    }

    private static async Task<IEnumerable<T>> ScanAll<T>(DynamoDBContext context, string table)
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
}

// var request = new QueryRequest
// {
//     TableName = "Reply",
//     KeyConditionExpression = "Id = :v_Id",
//     ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
//         {":v_Id", new AttributeValue { S =  "Amazon DynamoDB#DynamoDB Thread 1" }}}
// };
// var response = await client.QueryAsync(request);
// foreach (Dictionary<string, AttributeValue> item in response.Items)
// {
//     // Process the result.
//     // PrintItem(item);
// }
