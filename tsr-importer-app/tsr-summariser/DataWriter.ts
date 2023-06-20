import { DynamoDBClient, BatchWriteItemCommand } from '@aws-sdk/client-dynamodb';
import { marshall } from '@aws-sdk/util-dynamodb';
import { Summary } from './DataStructures';

export class DataWriter {
    public constructor(private dynamo: DynamoDBClient) {}

    public async write(table: string, summaries: Summary[]): Promise<boolean> {
        console.log(`Writing ${summaries.length} records to: ${table}...`);
        try {
            const chunkSize = 25;
            for (let i: number = 0; i < summaries.length; i += chunkSize) {
                const chunk = summaries.slice(i, i + chunkSize);
                console.debug(`Writing chunk: ${i}-${i + chunk.length}...`);

                // BackWriteItemCommand should replace existing items
                // primary key is the demographic code
                let request = new BatchWriteItemCommand({
                    RequestItems: {
                        [table]: chunk.map((summary) => {
                            return {
                                PutRequest: {
                                    Item: marshall(summary),
                                },
                            };
                        }),
                    },
                });
                console.debug('request', request);
                console.debug('first item', JSON.stringify(request.input.RequestItems![table][0]));
                let response = await this.dynamo.send(request);
                console.debug('response', response);
            }
            console.log('write complete');
            return true;
        } catch (err) {
            console.error(`error writing to: ${table}`);
            console.error(err);
            return false;
        }
    }
}
