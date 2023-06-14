import { QuestionTotals, Summary } from './DataStructures';

export class SessionScanner {
    private sessions: any[];

    constructor(sessions: any[]) {
        this.sessions = sessions;
    }

    public getUniqueDemographicCodes() {
        let codes = this.sessions
            .map((session) => session.participants)
            .flat()
            .map((participant) => this.createDemographicCode(participant))
            .filter((value, index, self) => self.indexOf(value) === index);
        console.log(`Found ${codes.length} unique demographic codes.`);
        return codes;
    }

    public getParticipantsForDemographic(code: string) {
        let participants = this.sessions
            .map((session) => session.participants)
            .flat()
            .filter((participant) => this.createDemographicCode(participant) === code);
        console.log(`Found ${participants.length} participants for demographic: ${code}`);
        return participants;
    }

    public generateQuestionCountsForDemographicSlice(
        code: string,
        participants: any[],
    ): { [key: string]: QuestionTotals } {
        let countsPerQuestion: { [key: string]: QuestionTotals } = {};
        participants.forEach((participant: any) => {
            participant.responses.forEach((response: any) => {
                if (!countsPerQuestion.hasOwnProperty(response.vote_id)) {
                    countsPerQuestion[response.vote_id] = {
                        demographic_code: code,
                        vote_id: response.vote_id,
                        stage_id: response.stage_id,
                        totals: {},
                        participants: 0,
                        max_boundary: response.max_boundary,
                        min_boundary: response.min_boundary,
                    };
                }
                let counts = countsPerQuestion[response.vote_id];
                counts.participants += 1;
                if (!counts.totals.hasOwnProperty(response.vote)) {
                    counts.totals[response.vote] = 0;
                }
                counts.totals[response.vote] += 1;
            });
        });
        return countsPerQuestion;
    }

    public createSummary(code: string, totals: { [key: string]: QuestionTotals }, participants: number) {
        let summary: Summary = {
            created: new Date().toISOString(),
            participants: participants,
            demographic: code,
            questionTotals: totals,
        };
        return summary;
    }

    private createDemographicCode(participant: any): string {
        return this.composeDemographicCode(
            participant.demographics.age_range,
            participant.demographics.ethnicity,
            participant.demographics.gender,
        );
    }

    private composeDemographicCode(age: string, ethnicity: string, gender: string): string {
        return `age=${age}:ethnicity=${ethnicity}:gender=${gender}`;
    }
}
