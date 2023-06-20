import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { SessionScanner } from './SessionScanner';
import { Summary } from './DataStructures';
import { DataReader } from './DataReader';
import { DataWriter } from './DataWriter';
import { DynamoDBStreamEvent } from 'aws-lambda';

interface Config {
    dataTableName: string;
    summaryTableName: string;
}

const dynamo: DynamoDBClient = new DynamoDBClient({
    region: process.env.AWS_REGION,
});

const config: Config = {
    dataTableName: process.env.TSR_DATA_PLATFORM_TABLE_NAME!,
    summaryTableName: process.env.TSR_SUMMARY_TABLE_NAME!,
};

const reader = new DataReader(dynamo);
const writer = new DataWriter(dynamo);

/**
 * Summarises the content of TSR_DATA_PLATFORM_TABLE_NAME into TSR_SUMMARY_TABLE_NAME
 * @param event the DynamoDBStreamEvent that triggered this summarisation
 * @returns
 */
export const lambdaHandler = async (event: DynamoDBStreamEvent): Promise<boolean> => {
    console.debug(`DynamoDBClient ready`, dynamo.config);
    console.debug(`DynamoDBClient region`, await dynamo.config.region());

    try {
        // read all sessions - this could get pretty intense, it's all read into memory in one go
        // NB. size of lambda increased from default 129Mb to 1024Mb
        let sessions = await reader.read(config.dataTableName);
        let scanner = new SessionScanner(sessions);

        let totalParticipantCount = sessions.map((session) => session.participants).flat().length;
        console.log(`${sessions.length} sessions contain ${totalParticipantCount} participants`);

        // generate unique demographic codes, eg. "age=66-75:ethnicity=AnyOtherWhiteBackground:gender=male"
        // these demographic codes are exclusive: nobody should appear in more than one
        let codes = scanner.getUniqueDemographicCodes();
        console.log(`${codes.length} unique demographic codes`, JSON.stringify(codes));

        // derive demographic slices (participants per unique demographic)
        let participantSlices = Object.fromEntries(
            codes.map((code) => [code, scanner.getParticipantsForDemographic(code)]),
        );
        console.debug(
            `Demographic participant slices created`,
            JSON.stringify(codes.map((code) => `${code}: ${participantSlices[code].length} participants`)),
        );

        // confirm each participant is accounted for in a slice of some sort
        let slicedParticipantCount = Object.values(participantSlices).flat().length;
        if (slicedParticipantCount !== totalParticipantCount) {
            throw `Slices contain ${slicedParticipantCount} participants, but sessions contain ${totalParticipantCount} participants.`;
        } else {
            console.debug('Sliced participant count matches the sessions participant count.');
        }

        // generate summary for each demographic
        // each question has a spread of results (eg. '0'-'10') for each combo demographic
        // eg. "f3-q1" has some results like { "1":5, ..., "9":4, "10":3 }
        let summaries: Summary[] = [];
        codes.forEach((code) => {
            let questionCounts = scanner.generateQuestionCountsForDemographicSlice(code, participantSlices[code]);
            let participants = participantSlices[code].length;
            let summary = scanner.createSummary(code, questionCounts, participants);
            // console.debug(`Summary for demographic: ${code}`, summary);
            summaries.push(summary);
        });
        console.debug(`${summaries.length} summaries`, summaries);

        // write summaries to DynamoDB
        let success = await writer.write(config.summaryTableName, summaries);
        if (!success) {
            throw 'Failed to write summary data to database.';
        }

        console.log('Complete.');
        return true;
    } catch (err) {
        console.error(err);
        throw err;
    }
};
