using System.Globalization;
using System.Text.Json;
using CsvHelper;
using TsrStats.Entities.WorkshopSummary;

public class FileUtilities
{
    public static void SaveDictAsInsightValueCSV(IDictionary<string, object> data, string path)
    {
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data.ToList().Select(kvp => new { insight = kvp.Key, value = kvp.Value.ToString() }));
        }
    }

    public static void SaveCSV<T>(IEnumerable<T> data, string path)
    {
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords<T>(data);
        }
    }

    public static void SaveDictsCSV(IEnumerable<Dictionary<string, object>> records, string path)
    {
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            var headings = new List<string>(records.First().Keys);
            foreach (var heading in headings)
            {
                csv.WriteField(heading);
            }
            csv.NextRecord();
            foreach (var item in records)
            {
                foreach (var heading in headings)
                {
                    csv.WriteField(item[heading]);
                }

                csv.NextRecord();
            }
        }
    }

    public static void SaveListListCSV(IEnumerable<IEnumerable<object>> records, string path)
    {
        using (var writer = new StreamWriter(path))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            foreach (var row in records)
            {
                foreach (var item in row)
                {
                    csv.WriteField(item);
                }
                csv.NextRecord();
            }
        }
    }

    public static void SaveJson(JsonDocument document, string path)
    {
        var writerOptions = new JsonWriterOptions { Indented = true };
        using FileStream fs = File.Create(path);
        using var writer = new Utf8JsonWriter(fs, options: writerOptions);
        writer.WriteStartObject();
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }
        writer.WriteEndObject();
        writer.Flush();
    }

    public static void SaveAllToDirectoryByCouncil(IEnumerable<WorkshopSummary> data, string directory)
    {
        Directory.CreateDirectory(directory);

        File.WriteAllText(
            Path.Combine(directory, "all-workshops.json"),
            JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));

        var councils = data.Select(ws => ws.council).Distinct();
        foreach (var council in councils)
        {
            var councilPath = Path.Combine(directory, Utilities.NormaliseDemographic(council));
            Directory.CreateDirectory(councilPath);
            var councilWorkshops = data.Where(ws => ws.council == council);
            foreach (var workshop in councilWorkshops)
            {
                File.WriteAllText(
                    Path.Combine(councilPath, $"{Utilities.NormaliseDemographic(council)}-{workshop.sessionId}.json"),
                    JsonSerializer.Serialize(workshop, new JsonSerializerOptions() { WriteIndented = true }));
            }
        }
    }
}