export interface QuestionTotals {
    demographic_code: string;
    vote_id: string;
    stage_id: string;
    totals: { [key: string]: number };
    participants: number;
    max_boundary?: number;
    min_boundary?: number;
}

export interface CountsPerQuestion {
    [key: string]: QuestionTotals;
}

export interface DemographicSummary {
    id: string;
    created: string;
    timestamp: number;
    participants: number;
    demographic: string;
    questionTotals: CountsPerQuestion;
}

export interface WorkshopSummary {
    id: string;
    sessionId: string;
    demographic: string;
    created: string;
    datetime: number;
    participants: number;
    council: string;
    questionTotals: CountsPerQuestion;
}

export interface SessionSummary {
    id: string;
    timestamp: number;
    participants: number;
    sessionId: string;
    localAuthority: string;
    questionTotals: { [key: string]: QuestionTotals };
}
