import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DataReader } from './DataReader';
import { SliceReader } from './SliceReader';
import { DemographicQuery, QuestionTotals, Summary, SearchDetail, Demographics } from './DataStructures';

interface Config {
    summaryTableName: string;
}

const config: Config = {
    summaryTableName: process.env.TSR_SUMMARY_TABLE_NAME!,
};

const dynamo: DynamoDBClient = new DynamoDBClient({
    region: process.env.AWS_REGION,
});

const dataReader = new DataReader(dynamo);

console.log('Fetching data....');
const preReadSlices = dataReader.read(config.summaryTableName).then((data) => {
    console.log(`${data.length} slices retrieved.`);
    return data;
});

const queryCache: { [key: string]: SearchDetail } = {};

const ALL_HEADERS = {
    'Access-Control-Allow-Headers': 'Content-Type',
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'OPTIONS,GET',
};

export const lambdaHandler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
    // options is the special verb for pre-flight checks - don't get data, just return headers
    if (event.httpMethod === 'OPTIONS') {
        return {
            statusCode: 200,
            headers: ALL_HEADERS,
            body: JSON.stringify({
                success: true,
            }),
        };
    }

    let start = Date.now();
    let query: DemographicQuery = {
        council: event.queryStringParameters?.council ?? '*',
        age: event.queryStringParameters?.age ?? '*',
        ethnicity: event.queryStringParameters?.ethnicity ?? '*',
        gender: event.queryStringParameters?.gender ?? '*',
    };
    console.log(`Query`, JSON.stringify(query));

    try {
        console.debug('Raw data read begin...');
        let slices = await preReadSlices;
        console.debug('read complete.');
        let sliceReader = new SliceReader(slices);

        // sarch description code is a demographic code with wildcards for missing parameters
        let searchDescriptionCode = sliceReader.composeDemographicCode(
            query.council,
            query.age,
            query.ethnicity,
            query.gender,
        );
        console.log(`Search description code: ${searchDescriptionCode}`);

        // perform a search if not already cached
        if (queryCache[searchDescriptionCode] === undefined) {
            queryCache[searchDescriptionCode] = sliceReader.searchAndSummarise(query, searchDescriptionCode);
        }

        // prepare and return the response
        let searchDetail = queryCache[searchDescriptionCode];
        return {
            statusCode: 200,
            headers: ALL_HEADERS,
            body: JSON.stringify({
                success: true,
                duration_ms: Date.now() - start,
                message: `Retrieved totals across demographic: ${searchDescriptionCode}`,
                query: query,
                all_demographics: searchDetail.demographics,
                included_demographics: {
                    found: searchDetail.foundDemographics,
                    not_found: searchDetail.notFoundDemographics,
                    total: searchDetail.foundDemographics.length + searchDetail.notFoundDemographics.length,
                },
                summary: searchDetail.summary,
            }),
        };
    } catch (err) {
        console.error(err);
        return {
            statusCode: 500,
            headers: ALL_HEADERS,
            body: JSON.stringify({
                duration_ms: Date.now() - start,
                success: false,
                message: 'An error occurred retrieving slice data',
                query: query,
                error: err,
            }),
        };
    }
};
