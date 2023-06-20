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
                let vote = response.vote as string;
                counts.participants += 1;
                if (!counts.totals.hasOwnProperty(response.vote)) {
                    counts.totals[vote] = 0;
                }
                counts.totals[vote] += 1;
                // console.log(`counts for ${response.vote_id}`, counts);
                // console.log(`countsPerQuestion[${response.vote_id}]`, countsPerQuestion[response.vote_id]);
            });
        });
        return countsPerQuestion;
    }

    public createSummary(code: string, totals: { [key: string]: QuestionTotals }, participants: number) {
        let summary: Summary = {
            id: code,
            created: new Date().toISOString(),
            timestamp: Date.now(),
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

    public composeDemographicCode(age: string, ethnicity: string, gender: string): string {
        return `age=${age}:ethnicity=${this.composeEthnicityCode(ethnicity)}:gender=${gender}`;
    }

    public composeEthnicityCode(ethnicity: string): string {
        return ethnicity
            .replace(/\w+/g, function (word) {
                return word.charAt(0).toUpperCase() + word.substring(1);
            })
            .replace(/\s/g, '')
            .replace(/-/g, '')
            .replace(/\"/g, '');
    }
}
