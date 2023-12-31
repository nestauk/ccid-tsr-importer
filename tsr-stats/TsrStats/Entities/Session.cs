
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CsvHelper.Configuration.Attributes;

public class Session
{
    [DynamoDBProperty("session-id")]
    [Index(0)] public string? sessionId { get; set; }

    [DynamoDBHashKey]
    [Index(1)] public string? id { get; set; }

    [Index(2)] public string? council { get; set; }
    [Index(3)] public long? datetime { get; set; }
    [Index(4)] public string? date { get; set; }

    [DynamoDBProperty(Converter = typeof(DocumentConverter))]
    [Ignore][Index(5)] public Document? modules { get; set; }

    [DynamoDBProperty(Converter = typeof(DocumentListConverter))]
    [Ignore][Index(6)] public List<Document>? participants { get; set; }

    [DynamoDBProperty(Converter = typeof(DocumentListConverter))]
    [Ignore][Index(7)] public List<Document>? unfilterable_polls { get; set; }
}

public class DocumentConverter : IPropertyConverter
{
    public object? FromEntry(DynamoDBEntry entry) => entry as Document;
    public DynamoDBEntry? ToEntry(object value) => value as Document;
}

public class DocumentListConverter : IPropertyConverter
{
    public object? FromEntry(DynamoDBEntry entry) => entry.AsListOfDocument();
    public DynamoDBEntry? ToEntry(object value) => DynamoDBList.Create(value as List<Document>);
}