import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
import { DynamoDBStreamEvent } from 'aws-lambda';
import { DataReader } from './DataReader';
import { DemographicSummary, WorkshopSummary } from './DataStructures';
import { DataWriter } from './DataWriter';
import { SessionScanner } from './SessionScanner';

interface Config {
    sessionTableName: string;
    summaryTableName: string;
    individualWorkshopSummaryTableName: string;
}

const dynamo: DynamoDBClient = new DynamoDBClient({
    region: process.env.AWS_REGION,
});

const dynamoDoc = DynamoDBDocumentClient.from(dynamo);

const config: Config = {
    sessionTableName: process.env.TSR_SESSION_TABLE_NAME!,
    summaryTableName: process.env.TSR_SUMMARY_TABLE_NAME!,
    individualWorkshopSummaryTableName: process.env.TSR_INDIVIDUAL_WORKSHOP_SUMMARY_TABLE_NAME!,
};

const checkConfig = () => {
    if (config.sessionTableName === undefined) {
        throw 'TSR_SESSION_TABLE_NAME is not defined.';
    }
    if (config.summaryTableName === undefined) {
        throw 'TSR_SUMMARY_TABLE_NAME is not defined.';
    }
    if (config.individualWorkshopSummaryTableName === undefined) {
        throw 'TSR_INDIVIDUAL_WORKSHOP_SUMMARY_TABLE_NAME is not defined.';
    }
};

const reader = new DataReader(dynamo);
const writer = new DataWriter(dynamo, dynamoDoc);

/**
 * Summarises the content of TSR_SESSION_TABLE_NAME into TSR_SUMMARY_TABLE_NAME
 */
const writeIndividualWorkshopSummaries = async (sessions: any[]) => {
    let summaries = getIndividualWorkshopSummaries(sessions);

    // write summaries to DynamoDB
    let success = await writer.writeIndividualWorkshopSummaryTable(
        config.individualWorkshopSummaryTableName,
        summaries,
    );
    if (!success) {
        throw 'Failed to write individual workshop summary data to database.';
    }
};

const getIndividualWorkshopSummaries = (sessions: any[]): WorkshopSummary[] => {
    return sessions
        .map((session) => {
            const scanner = new SessionScanner([session]);
            return {
                id: session['session-id'],
                council: session['council'],
                summaries: getDemographicSummaries([session], scanner, true),
            };
        })
        .map((perSession) =>
            perSession.summaries.map((summary) => toWorkshopSummary(perSession.id, perSession.council, summary)),
        )
        .flat();
};

const toWorkshopSummary = (
    sessionId: string,
    council: string,
    demographicSummary: DemographicSummary,
): WorkshopSummary => {
    return {
        id: `${sessionId}/${demographicSummary.demographic}`,
        sessionId: sessionId,
        demographic: demographicSummary.demographic,
        created: demographicSummary.created,
        timestamp: demographicSummary.timestamp,
        participants: demographicSummary.participants,
        council: council,
        questionTotals: demographicSummary.questionTotals,
    };
};

/**
 * Summarises the content of TSR_SESSION_TABLE_NAME into TSR_SUMMARY_TABLE_NAME
 */
const writeDemographicSummaries = async (sessions: any[]) => {
    let scanner = new SessionScanner(sessions);
    let summaries = getDemographicSummaries(sessions, scanner, false);

    // write summaries to DynamoDB
    let success = await writer.writeSummaryTable(config.summaryTableName, summaries);
    if (!success) {
        throw 'Failed to write demographic summary data to database.';
    }
};

const getDemographicSummaries = (
    sessions: any[],
    scanner: SessionScanner,
    lumpDemographics: boolean,
): DemographicSummary[] => {
    let totalParticipantCount = sessions.map((session) => session.participants).flat().length;
    console.log(`${sessions.length} sessions contain ${totalParticipantCount} participants`);

    // generate unique demographic codes, eg. "council=something:age=66-75:ethnicity=AnyOtherWhiteBackground:gender=male"
    // these demographic codes are exclusive: nobody should appear in more than one
    let codes = lumpDemographics ? ['*'] : scanner.getUniqueDemographicCodes();
    console.log(`${codes.length} unique demographic codes`, JSON.stringify(codes));

    // derive demographic slices (participants per unique demographic)
    let participantSlices = Object.fromEntries(
        codes.map((code) => [code, scanner.getParticipantsForDemographic(code)]),
    );

    console.debug(
        `${Object.entries(participantSlices).length} demographic participant slices created`,
        JSON.stringify(codes.map((code) => `${code}: ${participantSlices[code].length} participants`)),
    );

    // sum across the slices
    let sumAcrossSlices = codes.reduce((acc: number, code: string) => participantSlices[code].length + acc, 0);
    console.debug(`Sum of participants across slices: ${sumAcrossSlices}`);

    console.debug(
        `${Object.entries(participantSlices).length} demographic participant slices created`,
        JSON.stringify(codes.map((code) => `${code}: ${participantSlices[code].length} participants`)),
    );

    // confirm each participant is accounted for in a slice of some sort
    let slicedParticipantCount = Object.values(participantSlices).flat().length;
    if (slicedParticipantCount !== totalParticipantCount) {
        throw `Slices contain ${slicedParticipantCount} participants, but sessions contain ${totalParticipantCount} participants.`;
    } else {
        console.info('Sliced participant count matches the sessions participant count.');
    }

    // generate summary for each demographic
    // each question has a spread of results (eg. '0'-'10') for each combo demographic
    // eg. "f3-q1" has some results like { "1":5, ..., "9":4, "10":3 }
    let summaries: DemographicSummary[] = [];
    codes.forEach((code) => {
        let questionCounts = scanner.generateQuestionCountsForDemographicSlice(code, participantSlices[code]);
        let participants = participantSlices[code].length;
        let summary = scanner.createDemographicSummary(code, questionCounts, participants);
        // console.debug(`Summary for demographic: ${code}`, summary);
        summaries.push(summary);
    });
    console.info(`${summaries.length} summaries`);

    return summaries;
};

const readAllSessions = async () => {
    let sessions = await reader.readSessionTable(config.sessionTableName);
    if (sessions.length === 0) {
        throw 'No sessions found - not continuing with summarisation.';
    }
    return sessions;
};

/**
 * @param event the DynamoDBStreamEvent that triggered this summarisation
 * @returns
 */
export const lambdaHandler = async (event: DynamoDBStreamEvent): Promise<boolean> => {
    console.debug(`DynamoDBClient ready`, dynamo.config);
    console.debug(`DynamoDBClient region`, await dynamo.config.region());

    try {
        checkConfig();
        // read all sessions - this could get pretty intense, it's all read into memory in one go
        // NB. size of lambda increased from default 129Mb to 1024Mb
        console.log('*** Reading data...');
        const sessions = await readAllSessions();
        console.log(`${sessions.length} sessions found.`);

        console.log('*** Writing demographic summaries...');
        await writeDemographicSummaries(sessions);
        console.log('Demographic summaries complete.');

        console.log('*** Writing individual workshop summaries...');
        await writeIndividualWorkshopSummaries(sessions);
        console.log('Individual workshop summaries complete.');

        return true;
    } catch (err) {
        console.error(err);
        throw err;
    }
};
