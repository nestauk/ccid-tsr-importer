import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { ScanCommand } from '@aws-sdk/lib-dynamodb';

export class DataReader {
    public constructor(private dynamo: DynamoDBClient) {}

    public async read(table: string): Promise<any[]> {
        console.log(`Reading all records from: ${table}...`);
        let results: any[] = [];
        let request = new ScanCommand({ TableName: table });
        let finished: boolean = false;
        do {
            let response = await this.dynamo.send(request);
            console.log(`Found ${response.Items!.length} records in: ${table}`);
            results = results.concat(response.Items!);
            request.input.ExclusiveStartKey = response.LastEvaluatedKey;
            finished = response.LastEvaluatedKey === undefined;
        } while (!finished);
        return results;
    }
}
