using System.Globalization;
using System.Text.Json;
using CsvHelper;

public class FileUtilities
{
    public static void SaveDictCSV(IDictionary<string, object> data, string path)
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

}