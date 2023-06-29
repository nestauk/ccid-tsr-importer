using System.Globalization;
using CsvHelper;

public class DownloadHelper
{
    public static async Task<IEnumerable<T>> DownloadCsvAsync<T>(string url)
    {
        using (var client = new HttpClient())
        {
            using (var stream = await client.GetStreamAsync(url))
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        return csv.GetRecords<T>().ToList();
                    }
                }
            }
        }
    }

}