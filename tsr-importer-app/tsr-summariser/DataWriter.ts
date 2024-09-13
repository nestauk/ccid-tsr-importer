import { BatchWriteItemCommand, DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import { marshall } from '@aws-sdk/util-dynamodb';
import { DemographicSummary, WorkshopSummary } from './DataStructures';

export class DataWriter {
    public constructor(private dynamo: DynamoDBClient, private dynamoDoc: DynamoDBDocumentClient) {}

    public async writeSummaryTable(table: string, summaries: DemographicSummary[]): Promise<boolean> {
        return await this.writeTo(table, summaries);
    }

    public async writeIndividualWorkshopSummaryTable(table: string, summaries: WorkshopSummary[]): Promise<boolean> {
        return await this.writeTo(table, summaries);
    }

    private async writeTo(table: string, summaries: any[]): Promise<boolean> {
        if (!this.dynamoDoc) {
            throw 'DynamoDB documenting client not available';
        }
        console.log(`Writing ${summaries.length} records to: ${table}`);
        try {
            const chunkSize = 10;
            for (let i = 0; i < summaries.length; i += chunkSize) {
                const chunk = summaries.slice(i, i + chunkSize);

                // BatchWriteItemCommand should replace existing items
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
                console.debug(`Writing chunk: ${i}-${i + chunk.length}, ${chunk.length} items...`);
                let response = await this.dynamo.send(request);
                console.debug(`Response code: ${response.$metadata.httpStatusCode}`);
            }
            return true;
        } catch (err: any) {
            console.error(`Error writing to: ${table}`);
            console.error(err);
            throw err;
        } finally {
            console.log('Write complete');
        }
    }
}
