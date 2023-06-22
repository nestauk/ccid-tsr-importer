import { QuestionTotals, Summary } from './DataStructures';

export class SessionScanner {
    private sessions: any[];

    constructor(sessions: any[]) {
        this.sessions = sessions;
    }

    public getUniqueDemographicCodes() {
        let codes = this.sessions
            .map((session) =>
                session.participants.map((ppt: any) => {
                    // assign the session council to each participant
                    ppt.council = session.council;
                    return ppt;
                }),
            )
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
            // .checkbox is for multiple choice polls
            // map it as if it's a vote where checkbox choices replace the numeric options
            participant.checkbox.forEach((checkbox: any) => {
                let participant_polls: string[] = [];
                Object.keys(checkbox.result).forEach((key) => {
                    let parts = key.split('_');
                    let checkbox_vote_id = parts[0];
                    let checkbox_vote_yes = `${parts[1]}-true`;
                    let checkbox_vote_no = `${parts[1]}-false`;

                    // special case for t14 - these checkboxes are named c1, c2
                    if (/^c\d+/.test(checkbox_vote_id)) {
                        // keep the checkbox value as the vote choice
                        // set the vote id to t14 - it should always have been this
                        checkbox_vote_yes = `${checkbox_vote_id}-true`;
                        checkbox_vote_no = `${checkbox_vote_id}-false`;
                        checkbox_vote_id = `t14`;
                    }

                    // track which polls a participant has participated in
                    // this way we only increment the count once per poll
                    if (!participant_polls.includes(checkbox_vote_id)) {
                        participant_polls.push(checkbox_vote_id);
                    }

                    if (!countsPerQuestion.hasOwnProperty(checkbox_vote_id)) {
                        countsPerQuestion[checkbox_vote_id] = {
                            demographic_code: code,
                            vote_id: checkbox_vote_id,
                            stage_id: checkbox.stage_id,
                            totals: {},
                            participants: 0,
                        };
                    }
                    let counts = countsPerQuestion[checkbox_vote_id];
                    if (!counts.totals.hasOwnProperty(checkbox_vote_yes)) {
                        counts.totals[checkbox_vote_yes] = 0;
                    }
                    if (!counts.totals.hasOwnProperty(checkbox_vote_no)) {
                        counts.totals[checkbox_vote_no] = 0;
                    }
                    if (checkbox.result[key] === true) {
                        counts.totals[checkbox_vote_yes] += 1;
                    }
                    if (checkbox.result[key] === false) {
                        counts.totals[checkbox_vote_no] += 1;
                    }
                });

                // now increment the participant count for the polls they took part in
                console.debug(`Participant took part in ${participant_polls.length} polls`, participant_polls);
                participant_polls.forEach((poll_vote_id) => {
                    console.debug(`Incrementing participant count for ${poll_vote_id}`, countsPerQuestion);
                    countsPerQuestion[poll_vote_id].participants += 1;
                });
            });

            // .responses is for numeric polls
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
            participant.council,
            participant.demographics.age_range,
            participant.demographics.ethnicity,
            participant.demographics.gender,
        );
    }

    public composeDemographicCode(council: string, age: string, ethnicity: string, gender: string): string {
        let cc = this.normaliseCouncil(council);
        let ag = this.normaliseAgeRange(age);
        let et = this.normaliseEthnicityCode(ethnicity);
        let gd = this.normaliseGender(gender);
        return `council=${cc}:age=${ag}:ethnicity=${et}:gender=${gd}`;
    }

    public normaliseCouncil(council: string): string {
        return council.includes(' ') || council.includes('"') ? this.tidyWords(council) : council;
    }

    public normaliseAgeRange(age: string): string {
        return age.toLowerCase().trim();
    }

    public normaliseEthnicityCode(ethnicity: string): string {
        return ethnicity.includes(' ') || ethnicity.includes('"') ? this.tidyWords(ethnicity) : ethnicity;
    }

    public normaliseGender(gender: string): string {
        return gender.toLowerCase().trim();
    }

    private tidyWords(words: string): string {
        return words
            .toLowerCase()
            .trim()
            .replace(/\w+/g, function (word) {
                return word.charAt(0).toUpperCase() + word.substring(1);
            })
            .replace(/\s/g, '')
            .replace(/-/g, '')
            .replace(/\"/g, '');
    }
}
