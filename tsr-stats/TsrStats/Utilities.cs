public class Utilities
{
    public static string UnixTimeStampToDate(long timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime().Date.ToString("yyyy-MM-dd");

}