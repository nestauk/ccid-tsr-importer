
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

public class Session
{
    [DynamoDBHashKey]
    [DynamoDBProperty("session-id")]
    public string? sessionId { get; set; }

    public string? council { get; set; }
    public long? datetime { get; set; }
    public Document? modules { get; set; }
    public List<Document>? participants { get; set; }
    public List<Document>? unfilterable_polls { get; set; }
}