using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper;

public class S3Scanner
{

    public static async Task<IEnumerable<S3Session>> ScanS3Async(AmazonS3Client client, string bucket, string prefix)
    {
        var paginator = client.Paginators.ListObjectsV2(new ListObjectsV2Request
        {
            BucketName = bucket,
            Prefix = prefix
        });

        var entries = new List<Tuple<string, long>>();
        await foreach (var response in paginator.Responses)
        {
            foreach (var entry in response.S3Objects)
            {
                entries.Add(new Tuple<string, long>(entry.Key, entry.Size));
            }
        }

        var path_keys = entries
            .Where(e => e.Item1.StartsWith(prefix))
            .Where(e => e.Item1.EndsWith(".zip"))
            .Select(e => e.Item1[prefix.Length..].Split('/')[0]).Distinct();

        var sessions = new List<S3Session>();
        var errors = new Dictionary<string, List<string>>();
        foreach (var path_key in path_keys)
        {
            var file_with_repaired = entries
                .Where(e => e.Item1.Contains($"/{path_key}/"))
                .Where(e => e.Item1.EndsWith(".zip"))
                .OrderByDescending(e => e.Item1).FirstOrDefault();

            var file_with_prefix = entries
                .Where(e => e.Item1.Contains($"/{path_key}/"))
                .Where(e => e.Item1.EndsWith(".zip"))
                .Where(e => e.Item1.Contains($"/{path_key}/{path_key}-"))
                .OrderByDescending(e => e.Item1).FirstOrDefault();

            var latest_timestamp_file = entries
                .Where(e => e.Item1.Contains($"/{path_key}/"))
                .Where(e => e.Item1.EndsWith(".zip"))
                .OrderByDescending(e => e.Item1).FirstOrDefault();

            var largest_file_path = entries
                .Where(e => e.Item1.Contains($"/{path_key}/"))
                .Where(e => e.Item1.EndsWith(".zip"))
                .OrderByDescending(e => e.Item2).FirstOrDefault();

            var selected_file_path = file_with_repaired ?? file_with_prefix ?? latest_timestamp_file ?? largest_file_path;

            using (var stream = await GetS3StreamAsync(client, bucket, selected_file_path!.Item1))
            {
                try
                {
                    using var zip = new ZipArchive(stream);

                    var has_files = new
                    {
                        stage_slider_vote_votes = zip.Entries.Any(e => e.Name.EndsWith("stage_slider_vote_votes.csv")),
                        stage_text_input_votes = zip.Entries.Any(e => e.Name.EndsWith("stage_text_input_votes.csv")),
                        stage_timings = zip.Entries.Any(e => e.Name.EndsWith("stage_timings.csv")),
                        user_demographics = zip.Entries.Any(e => e.Name.EndsWith("user_demographics.csv")),
                        stage_slider_vote_results = zip.Entries.Any(e => e.Name.EndsWith("stage_slider_vote_results.csv")),
                        stage_checkbox_votes = zip.Entries.Any(e => e.Name.EndsWith("stage_checkbox_votes.csv"))
                    };

                    var votes_entry = FindBestFileInZip(zip, "stage_slider_vote_votes.csv");
                    var text_inputs_entry = FindBestFileInZip(zip, "stage_text_input_votes.csv");
                    var timings_entry = FindBestFileInZip(zip, "stage_timings.csv");
                    var user_demographics_entry = FindBestFileInZip(zip, "user_demographics.csv");

                    var slider_vote_votes = votes_entry != null ? ReadCSV<StageSliderVoteVotes>(votes_entry.Open(), votes_entry.Name) : null;
                    var text_inputs = text_inputs_entry != null ? ReadCSV<StageTextInputVotes>(text_inputs_entry.Open(), text_inputs_entry.Name) : null;
                    var timings = timings_entry != null ? ReadCSV<StageTimings>(timings_entry.Open(), timings_entry.Name) : null;
                    var demographics = user_demographics_entry != null ? ReadCSV<UserDemographics>(user_demographics_entry.Open(), user_demographics_entry.Name) : null;

                    var council = text_inputs?.First(ti => ti.stage_id == "local-authority").vote;
                    var session_id = text_inputs?.First(ti => ti.stage_id == "local-authority").session_id;
                    var any_timestamp = timings?.First().end_time;
                    var date = any_timestamp != null ? Utilities.UnixTimeStampToDate(any_timestamp!.Value) : null;
                    var participants = slider_vote_votes?.Select(r => r.cast_uuid).Distinct();
                    var unique_age_ranges = demographics?.Select(d => d.age_range).Where(ar => !string.IsNullOrWhiteSpace(ar)).Distinct();
                    var unique_ethnicities = demographics?.Select(d => d.ethnicity).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();
                    var unique_genders = demographics?.Select(d => d.gender).Where(g => !string.IsNullOrWhiteSpace(g)).Distinct();

                    sessions.Add(new S3Session
                    {
                        path_key = path_key.Trim(),
                        filename = selected_file_path.Item1,
                        participants = participants?.Count(),
                        council = council?.Trim(),
                        date = date?.Trim(),
                        session_id = session_id?.Trim(),
                        unique_age_ranges = JsonSerializer.Serialize(unique_age_ranges ?? Array.Empty<string>()),
                        unique_ethnicities = JsonSerializer.Serialize(unique_ethnicities ?? Array.Empty<string>()),
                        unique_genders = JsonSerializer.Serialize(unique_genders ?? Array.Empty<string>()),
                        stage_slider_vote_votes = has_files.stage_slider_vote_votes,
                        stage_text_input_votes = has_files.stage_text_input_votes,
                        stage_timings = has_files.stage_timings,
                        user_demographics = has_files.user_demographics,
                        stage_slider_vote_results = has_files.stage_slider_vote_results,
                        stage_checkbox_votes = has_files.stage_checkbox_votes
                    });
                    Console.Write('.');
                }
                catch (Exception e)
                {
                    if (!errors.ContainsKey(path_key)) errors.Add(path_key, new List<string>());
                    errors[path_key].Add($"{e.GetType().Name}: {e.Message}:\n{e}");
                }
            } // memory stream
        }
        Console.WriteLine();
        Console.WriteLine($"{errors.Count} errors");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error.Key}: {string.Join(", ", error.Value)}");
        }

        Console.WriteLine();
        return sessions;
    }

    public static ZipArchiveEntry? FindBestFileInZip(ZipArchive zip, string filename)
    {
        return zip.Entries
            .Where(e => e.Name.EndsWith(filename) && !e.Name.StartsWith(".") && !e.Name.StartsWith("._") && !e.FullName.Contains("MACOSX"))
            .OrderByDescending(e => e.Length)
            .FirstOrDefault();
    }

    public static IEnumerable<T> ReadCSV<T>(Stream stream, string name)
    {
        try
        {
            using var streamReader = new StreamReader(stream);
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                return csvReader.GetRecords<T>().ToList();
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Error reading CSV: {name}, {e.GetType().Name}: {e.Message}", e);
        }
    }

    public static async Task<Stream> GetS3StreamAsync(AmazonS3Client client, string bucket, string key)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key,
        };
        var memoryStream = new MemoryStream();
        using var response = await client.GetObjectAsync(request);
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream;
    }

}