import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DataReader } from './DataReader';
import { Demographics, SliceReader } from './SliceReader';
import { QuestionTotals, Summary } from './DataStructures';

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

console.log('Pre-fetching data...');
const preReadSlices = dataReader.read(config.summaryTableName).then((data) => {
    console.log(`${data.length} slices retrieved.`);
    return data;
});

export const lambdaHandler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
    let start = Date.now();
    let query = {
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
        let demographics = sliceReader.getDemographics();
        console.debug('All demographics', sliceReader.getDemographics());

        let ages = query.age === '*' ? demographics.ages : [query.age];
        let ethnicities =
            query.ethnicity === '*' ? demographics.ethnicities : [sliceReader.normaliseEthnicityCode(query.ethnicity)];
        let genders = query.gender === '*' ? demographics.genders : [query.gender];

        // capture all summaries that match the query
        let summaries: Summary[] = [];
        let foundDemographics: string[] = [];
        let notFoundDemographics: string[] = [];
        for (let age of ages) {
            for (let ethnicity of ethnicities) {
                for (let gender of genders) {
                    let code = sliceReader.composeDemographicCode(age, ethnicity, gender);
                    console.log(`Searching for ${code}...`);
                    let foundSummary = slices.find((slice) => slice.demographic === code)!;
                    if (foundSummary !== undefined) {
                        summaries.push(foundSummary);
                        foundDemographics.push(code);
                    } else {
                        notFoundDemographics.push(code);
                    }
                }
            }
        }
        console.debug(`Found: ${foundDemographics.length} summaries.`, JSON.stringify(foundDemographics));
        console.debug(`Not found: ${notFoundDemographics.length} summaries.`, JSON.stringify(notFoundDemographics));
        console.debug(`Including ${summaries.length} summaries in summation.`, summaries);

        // sarch description code is a demographic code with wildcards for missing parameters
        let searchDescriptionCode = sliceReader.composeDemographicCode(query.age, query.ethnicity, query.gender);
        console.log(`Search description code: ${searchDescriptionCode}`);

        // identify unique vote_id values across all summaries
        let uniqueVoteIds = summaries
            .map((summary) => Object.keys(summary.questionTotals))
            .flat()
            .filter((value, index, self) => self.indexOf(value) === index);
        console.debug(`Found ${uniqueVoteIds.length} unique vote_ids`, uniqueVoteIds);

        // generate the sums for each option in the vote for each vote_id
        // (taken from the demographic matching matching summaries)
        let voteTotals: { [key: string]: QuestionTotals } = {};
        for (let voteId of uniqueVoteIds) {
            console.debug(`Summing vote_id ${voteId}...`);
            let foundTotalSetForVote = summaries
                .map((summary) => summary.questionTotals[voteId])
                .filter((value) => value !== undefined);
            console.debug(
                `Found ${foundTotalSetForVote.length} matching summaries for vote_id ${voteId}`,
                foundTotalSetForVote,
            );

            let summedTotals: QuestionTotals = {
                demographic_code: searchDescriptionCode,
                vote_id: voteId,
                stage_id: foundTotalSetForVote[0].stage_id, // should be the same across all
                max_boundary: foundTotalSetForVote[0].max_boundary, // should be the same across all
                min_boundary: foundTotalSetForVote[0].min_boundary, // should be the same across all
                participants: foundTotalSetForVote.reduce((acc, foundTotals) => acc + foundTotals.participants, 0),
                totals: {},
            };
            for (let i = summedTotals.min_boundary; i <= summedTotals.max_boundary; i++) {
                summedTotals.totals[i.toString()] = foundTotalSetForVote.reduce(
                    (acc, foundTotals) => acc + (foundTotals.totals[i.toString()] ?? 0),
                    0,
                );
            }
            voteTotals[voteId] = summedTotals;
        }

        // compose totals object
        let summary: Summary = {
            id: searchDescriptionCode,
            demographic: searchDescriptionCode,
            created: new Date().toISOString(),
            timestamp: new Date().getTime(),
            participants: summaries.reduce((acc, summary) => acc + summary.participants, 0),
            questionTotals: voteTotals,
        };

        return {
            statusCode: 200,
            body: JSON.stringify({
                success: true,
                duration_ms: Date.now() - start,
                message: `Retrieved totals across demographic: ${searchDescriptionCode}`,
                query: query,
                all_demographics: demographics,
                included_demographics: {
                    found: foundDemographics,
                    not_found: notFoundDemographics,
                    total: foundDemographics.length + notFoundDemographics.length,
                },
                summary: summary,
            }),
        };
    } catch (err) {
        console.error(err);
        return {
            statusCode: 500,
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
