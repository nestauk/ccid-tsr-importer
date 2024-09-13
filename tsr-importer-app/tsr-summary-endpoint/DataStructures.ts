export interface QuestionTotals {
    demographic_code: string;
    vote_id: string;
    stage_id: string;
    totals: { [key: string]: number };
    participants: number;
    max_boundary: number;
    min_boundary: number;
}

export interface DemographicSummary {
    id: string;
    created: string;
    timestamp: number;
    participants: number;
    demographic: string;
    questionTotals: { [key: string]: QuestionTotals };
}

export interface Demographics {
    ages: string[];
    ethnicities: string[];
    genders: string[];
    councils: string[];
    hr: HumanReadableDemographics;
    codes: string[];
}

export interface HumanReadableDemographics {
    ages: string[];
    ethnicities: string[];
    genders: string[];
    councils: string[];
}

export interface DemographicQuery {
    council: string;
    age: string;
    ethnicity: string;
    gender: string;
}

export interface SearchDetail {
    summary: DemographicSummary;
    demographics: Demographics;
    foundDemographics: string[];
    notFoundDemographics: string[];
}
