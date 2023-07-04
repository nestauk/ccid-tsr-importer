
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CsvHelper.Configuration.Attributes;

public class Session
{
    [DynamoDBHashKey]
    [DynamoDBProperty("session-id")]
    [Index(0)] public string? sessionId { get; set; }

    [Index(1)] public string? council { get; set; }
    [Index(2)] public long? datetime { get; set; }
    [Ignore][Index(3)] public Document? modules { get; set; }
    [Ignore][Index(4)] public List<Document>? participants { get; set; }
    [Ignore][Index(5)] public List<Document>? unfilterable_polls { get; set; }
}