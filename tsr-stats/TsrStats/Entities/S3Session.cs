using CsvHelper.Configuration.Attributes;

public class S3Session
{
    [Index(0)] public string? path_key { get; set; }
    [Index(1)] public string? filename { get; set; }
    [Index(2)] public string? session_id { get; set; }
    [Index(3)] public string? council { get; set; }
    [Index(4)] public string? date { get; set; }
    [Index(5)] public int? participants { get; set; }

    [Index(6)] public bool stage_text_input_votes { get; set; }
    [Index(7)] public bool user_demographics { get; set; }
    [Index(8)] public bool stage_slider_vote_votes { get; set; }
    [Index(9)] public bool stage_timings { get; set; }
    [Index(10)] public bool stage_slider_vote_results { get; set; }
    [Index(11)] public bool stage_checkbox_votes { get; set; }
}
