using Amazon.DynamoDBv2.DataModel;

public class DynamoScanner
{
    public static async Task<IEnumerable<T>> ScanAllAsync<T>(DynamoDBContext context, string table)
    {
        var items = new List<T>();
        var config = new DynamoDBOperationConfig
        {
            OverrideTableName = table,
        };

        var search = context.ScanAsync<T>(null, config);

        bool finished;
        do
        {
            items.AddRange(await search.GetNextSetAsync());
            finished = search.IsDone;
        } while (!finished);

        return items;
    }

}