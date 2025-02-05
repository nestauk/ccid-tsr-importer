using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TsrStats.Entities.WorkshopSummary;

public class WorkshopSummary
{
    public string id { get; set; } = null!;
    public string council { get; set; } = null!;
    public DateTime created { get; set; }
    public long datetime { get; set; } // apparently this is seconds
    public string demographic { get; set; } = null!;
    public int participants { get; set; }

    [DynamoDBProperty(Converter = typeof(QuestionTotalsConverter))]
    [JsonIgnore]
    public Document? questionTotals { get; set; }
    
    public string sessionId { get; set; } = null!;
    // public long timestamp { get; set; }
    // public DateTime workshopDate => DateTime.UnixEpoch.AddMilliseconds(timestamp);
    
    public object questionTotalsObject => JsonSerializer.Deserialize<object>(questionTotals!.ToJson())!;

    public string workshopDate => DateTime.UnixEpoch.AddSeconds(datetime).ToString("yyyy-MM-dd");
    public string workshopTime => DateTime.UnixEpoch.AddSeconds(datetime).TimeOfDay < TimeSpan.FromHours(12) ? "AM" : "PM";
    public string? workshopNumber { get; set; }
}

// Custom property converter for the Document objects
class QuestionTotalsConverter : IPropertyConverter
{
    public object FromEntry(DynamoDBEntry entry)
    {
        // Convert the DynamoDBEntry object to a Document object
        var document = entry as Document;
        return document;
    }

    public DynamoDBEntry ToEntry(object value)
    {
        // Convert the Document object to a DynamoDBEntry object
        var document = value as Document;
        return document;
    }
}