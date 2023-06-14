
export class SessionScanner {

    private sessions: any[];

    constructor(sessions: any[]) {
        this.sessions = sessions;
    }

    public getUniqueDemographicCodes() {
        return this.sessions
            .map(session => session.participants)
            .flat()
            .map(participant => this.createDemographicCode(participant))
            .filter((value, index, self) => self.indexOf(value) === index);
    }

    public getParticipantsForDemographic(code: string) {
        return this.sessions
            .map(session => session.participants)
            .flat()
            .filter(participant => this.createDemographicCode(participant) === code);
    }

    private createDemographicCode(participant: any): string {
        return this.composeDemographicCode(
            participant.demographics.age_range,
            participant.demographics.ethnicity,
            participant.demographics.gender);
    }

    private composeDemographicCode(age: string, ethnicity: string, gender: string): string {
        return `age=${age}:ethnicity=${ethnicity}:gender=${gender}`;
    }


}
