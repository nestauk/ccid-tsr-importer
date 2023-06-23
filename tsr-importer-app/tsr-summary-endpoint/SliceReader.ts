import { Summary, QuestionTotals, DemographicQuery, Demographics, SearchDetail } from './DataStructures';

export class SliceReader {
    public constructor(private slices: Summary[]) {}

    public getDemographics(): Demographics {
        let codes = this.slices.map((slice) => slice.demographic);
        let demographics: Demographics = {
            ages: [],
            ethnicities: [],
            genders: [],
            councils: [],
            codes: [],
        };
        for (let code of codes) {
            let parts = code.trim().split(':');
            if (parts.length !== 4) {
                throw `Invalid demographic code has ${parts.length} parts: ${code}`;
            }
            let council = this.normaliseCouncil(parts[0].split('=')[1]);
            let age = parts[1].split('=')[1];
            let ethnicity = this.normaliseEthnicityCode(parts[2].split('=')[1]);
            let gender = parts[3].split('=')[1];
            let normalisedCode = this.composeDemographicCode(council, age, ethnicity, gender);

            if (!demographics.councils.includes(council)) {
                demographics.councils.push(council);
            }
            if (!demographics.ages.includes(age)) {
                demographics.ages.push(age);
            }
            if (!demographics.ethnicities.includes(ethnicity)) {
                demographics.ethnicities.push(ethnicity);
            }
            if (!demographics.genders.includes(gender)) {
                demographics.genders.push(gender);
            }
            if (!demographics.codes.includes(normalisedCode)) {
                demographics.codes.push(normalisedCode);
            }
        }
        return demographics;
    }

    public getDemographicValues(query: DemographicQuery, demographics: Demographics): { [key: string]: string[] } {
        return {
            councils: query.council === '*' ? demographics.councils : [this.normaliseCouncil(query.council)],
            ages: query.age === '*' ? demographics.ages : [query.age],
            ethnicities:
                query.ethnicity === '*' ? demographics.ethnicities : [this.normaliseEthnicityCode(query.ethnicity)],
            genders: query.gender === '*' ? demographics.genders : [query.gender],
        };
    }

    public getSummariesForDemographics(
        slices: Summary[],
        councils: string[],
        ages: string[],
        ethnicities: string[],
        genders: string[],
    ): { summaries: Summary[]; foundDemographics: string[]; notFoundDemographics: string[] } {
        let summaries: Summary[] = [];
        let foundDemographics: string[] = [];
        let notFoundDemographics: string[] = [];
        for (let council of councils) {
            for (let age of ages) {
                for (let ethnicity of ethnicities) {
                    for (let gender of genders) {
                        let code = this.composeDemographicCode(council, age, ethnicity, gender);
                        console.log(`Searching for ${code}...`);
                        let foundSummary = slices.find((slice) => slice.demographic === code)!;
                        if (foundSummary !== undefined) {
                            summaries.push(foundSummary);
                            foundDemographics.push(code);
                        } else {
                            notFoundDemographics.push(code);
                        }
                    } // genders
                } // ethnicities
            } // ages
        } // councils
        return { summaries, foundDemographics, notFoundDemographics };
    }

    public getUniqueVoteIds(summaries: Summary[]): string[] {
        return summaries
            .map((summary) => Object.keys(summary.questionTotals))
            .flat()
            .filter((value, index, self) => self.indexOf(value) === index);
    }

    public computeVoteTotals(
        uniqueVoteIds: string[],
        summaries: Summary[],
        searchDescriptionCode: string,
    ): { [key: string]: QuestionTotals } {
        let voteTotals: { [key: string]: QuestionTotals } = {};
        for (let voteId of uniqueVoteIds) {
            console.debug(`Summing vote_id ${voteId}...`);
            let foundTotalSetForVote = summaries
                .map((summary) => summary.questionTotals[voteId])
                .filter((value) => value !== undefined);
            console.debug(
                `Found ${foundTotalSetForVote.length} matching summaries for vote_id ${voteId}`,
                foundTotalSetForVote,
            );

            let summedTotals: QuestionTotals = {
                demographic_code: searchDescriptionCode,
                vote_id: voteId,
                stage_id: foundTotalSetForVote[0].stage_id, // should be the same across all
                max_boundary: foundTotalSetForVote[0].max_boundary, // should be the same across all
                min_boundary: foundTotalSetForVote[0].min_boundary, // should be the same across all
                participants: foundTotalSetForVote.reduce((acc, foundTotals) => acc + foundTotals.participants, 0),
                totals: {},
            };

            // sum across all totals in the foundTotalSet
            foundTotalSetForVote.forEach((foundTotals) => {
                Object.keys(foundTotals.totals).forEach((key) => {
                    if (summedTotals.totals[key] === undefined) {
                        summedTotals.totals[key] = 0;
                    }
                    summedTotals.totals[key] += foundTotals.totals[key];
                });
            });

            // if (summedTotals.min_boundary === undefined || summedTotals.max_boundary === undefined) {
            //     foundTotalSetForVote.forEach((foundTotals) => {
            //         Object.keys(foundTotals.totals).forEach((key) => {
            //             if (summedTotals.totals[key] === undefined) {
            //                 summedTotals.totals[key] = 0;
            //             }
            //             summedTotals.totals[key] += foundTotals.totals[key];
            //         });
            //     });
            // } else {
            //     for (let i = summedTotals.min_boundary; i <= summedTotals.max_boundary; i++) {
            //         summedTotals.totals[i.toString()] = foundTotalSetForVote.reduce(
            //             (acc, foundTotals) => acc + (foundTotals.totals[i.toString()] ?? 0),
            //             0,
            //         );
            //     }

            //     // special case - the numeric votes may also have a recommended/not-recommended/neutral value we should get hold of, too
            //     if (summedTotals.totals.hasOwnProperty('recommended')) {
            //         ['not-recommended', 'recommended', 'neutral'].forEach((position) => {
            //             summedTotals.totals[position] = foundTotalSetForVote.reduce(
            //                 (acc, foundTotals) => acc + (foundTotals.totals[position] ?? 0),
            //                 0,
            //             );
            //         });
            //     }
            // }
            voteTotals[voteId] = summedTotals;
        }
        return voteTotals;
    }

    public searchAndSummarise(query: DemographicQuery, searchDescriptionCode: string): SearchDetail {
        let demographics = this.getDemographics();
        console.debug('All demographics', this.getDemographics());

        // define all the possible values of the demographic variables for the search
        let { ages, ethnicities, genders, councils } = this.getDemographicValues(query, demographics);

        // capture all summaries that match the query
        let { summaries, foundDemographics, notFoundDemographics } = this.getSummariesForDemographics(
            this.slices,
            councils,
            ages,
            ethnicities,
            genders,
        );
        console.debug(`Found: ${foundDemographics.length} summaries.`, JSON.stringify(foundDemographics));
        console.debug(`Not found: ${notFoundDemographics.length} summaries.`, JSON.stringify(notFoundDemographics));
        console.debug(`Including ${summaries.length} summaries in summation.`, summaries);

        // identify unique vote_id values across all summaries
        let uniqueVoteIds = this.getUniqueVoteIds(summaries);
        console.debug(`Found ${uniqueVoteIds.length} unique vote_ids`, uniqueVoteIds);

        // generate the sums for each option in the vote for each vote_id
        // (taken from the demographic matching matching summaries)
        let voteTotals = this.computeVoteTotals(uniqueVoteIds, summaries, searchDescriptionCode);

        // compose summary
        let summary: Summary = {
            id: searchDescriptionCode,
            demographic: searchDescriptionCode,
            created: new Date().toISOString(),
            timestamp: new Date().getTime(),
            participants: summaries.reduce((acc, summary) => acc + summary.participants, 0),
            questionTotals: voteTotals,
        };

        return { summary, demographics, foundDemographics, notFoundDemographics };
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
