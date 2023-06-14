export interface QuestionTotals {
    demographic_code: string;
    vote_id: string;
    stage_id: string;
    totals: { [key: string]: number };
    participants: number;
    max_boundary: number;
    min_boundary: number;
}

export interface Summary {
    created: string;
    participants: number;
    demographic: string;
    questionTotals: { [key: string]: QuestionTotals };
}
