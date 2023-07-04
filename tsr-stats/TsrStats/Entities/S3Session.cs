using CsvHelper.Configuration.Attributes;

public class S3Session
{
    [Index(0)] public string? path_key { get; set; }
    [Index(1)] public string session_id { get; set; }
    [Index(2)] public string? council { get; set; }
    [Index(3)] public DateTime? date { get; set; }
    [Index(4)] public int participants { get; set; }

}