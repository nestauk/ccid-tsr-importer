using CsvHelper.Configuration.Attributes;

public class Summary
{
    [Index(0)] public string? id { get; set; }
    [Index(1)] public DateTime? created { get; set; }
    [Index(2)] public string? demographic { get; set; }
    [Index(3)] public int? participants { get; set; }
    [Index(4)] public long? timestamp { get; set; }
}