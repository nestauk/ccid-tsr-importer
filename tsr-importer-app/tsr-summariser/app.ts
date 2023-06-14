import { APIGatewayProxyEvent, APIGatewayProxyResult } from 'aws-lambda';
import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { ScanCommand } from '@aws-sdk/lib-dynamodb';
import { SessionScanner } from './SessionScanner';
import { QuestionTotals, Summary } from './DataStructures';

interface Config {
    dataTableName: string;
    summaryTableName: string;
}

let dynamo: DynamoDBClient;

const read = async (table: string): Promise<any[]> => {
    console.log(`Reading all records from: ${table}...`);
    let results: any[] = [];
    let request = new ScanCommand({ TableName: table });
    let finished: boolean = false;
    do {
        let response = await dynamo.send(request);
        console.log(`Found ${response.Items!.length} records in: ${table}`);
        results = results.concat(response.Items!);
        request.input.ExclusiveStartKey = response.LastEvaluatedKey;
        finished = response.LastEvaluatedKey === undefined;
    } while (!finished);
    return results;
};

export const lambdaHandler = async (event: APIGatewayProxyEvent): Promise<APIGatewayProxyResult> => {
    try {
        dynamo = new DynamoDBClient({
            region: process.env.AWS_REGION,
        });
        const config: Config = {
            dataTableName: process.env.TSR_DATA_PLATFORM_TABLE_NAME!,
            summaryTableName: process.env.TSR_SUMMARY_TABLE_NAME!,
        };

        // read all sessions - this could get pretty intense, it's not streamed
        let sessions = await read(config.dataTableName);
        let scanner = new SessionScanner(sessions);

        let totalParticipantCount = sessions.map((session) => session.participants).flat().length;
        console.log(`sessions contain ${totalParticipantCount} participants`);

        // generate unique demographic codes, eg. "20-30:W:M"
        // these demographic codes are exclusive: nobody can appear in more than one
        // NB. some ethnicities start with an unexpected inverted commas - this is going to come back and bite us in the ass
        // NB. ignore first half postcode, it's not one of the searchable demographics (for now)
        let codes = scanner.getUniqueDemographicCodes();
        console.log('Unique demographic codes', codes);

        // derive demographic slices (participants per unique demographic)
        let participantSlices = Object.fromEntries(
            codes.map((code) => [code, scanner.getParticipantsForDemographic(code)]),
        );

        // confirm each participant is accounted for in a slice of some sort
        let slicedParticipantCount = Object.values(participantSlices).flat().length;
        if (slicedParticipantCount !== totalParticipantCount) {
            console.warn(`slices contain ${slicedParticipantCount} participants`);
        }

        // generate summary for each demographic
        codes.forEach((code) => {
            let questionCounts = scanner.generateQuestionCountsForDemographicSlice(code, participantSlices[code]);
            let participants = participantSlices[code].length;
            let summary = scanner.createSummary(code, questionCounts, participants);
            console.log(`Summary for ${code}`, summary);
        });

        // each question has a spread of results (eg. 0-10) for each combo demographic
        // eg. "f3-q1" has some results like { "1":5, ..., "9":4, "10":3 }

        // TODO: collate counts per demographic for each value for each question
        // for each demographic code
        // assemble the participants that match the code across ALL sessions
        // for each question
        // total the votes for the question id
        // ie. where: participant.responses[].vote_id
        //     sum: participant.responses[].vote
        // capture the top and bottom values
        // participant.responses[].max_boundary
        // participant.responses[].min_boundary
        // create a new demographic slice record:
        /* { 
                            "demographic_code": code, 
                            "vote_id": vote_id,
                            "totals": { "1": 5, ..., "10": 3 },
                            "max_boundary": 10,
                            "min_boundary": 1 
                        } */

        return {
            statusCode: 200,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                message: 'ok', // TODO: replace with summary of activity
            }),
        };
    } catch (err) {
        console.log(err);
        return {
            statusCode: 500,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                message: err || 'Unexpected error summarising data',
            }),
        };
    }
};
