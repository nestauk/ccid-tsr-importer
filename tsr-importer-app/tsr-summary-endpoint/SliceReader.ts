import { Summary, QuestionTotals } from './DataStructures';

export interface Demographics {
    ages: string[];
    ethnicities: string[];
    genders: string[];
    codes: string[];
}

export class SliceReader {
    public constructor(private slices: Summary[]) {}

    public getDemographics(): Demographics {
        let codes = this.slices.map((slice) => slice.demographic);
        let demographics: Demographics = {
            ages: [],
            ethnicities: [],
            genders: [],
            codes: codes,
        };
        for (let code of codes) {
            let parts = code.trim().split(':');
            if (parts.length !== 3) {
                throw `Invalid demographic code has ${parts.length} parts: ${code}`;
            }
            let age = parts[0].split('=')[1];
            let ethnicity = this.normaliseEthnicityCode(parts[1].split('=')[1]);
            let gender = parts[2].split('=')[1];

            if (!demographics.ages.includes(age)) {
                demographics.ages.push(age);
            }
            if (!demographics.ethnicities.includes(ethnicity)) {
                demographics.ethnicities.push(ethnicity);
            }
            if (!demographics.genders.includes(gender)) {
                demographics.genders.push(gender);
            }
        }
        return demographics;
    }

    public composeDemographicCode(age: string, ethnicity: string, gender: string): string {
        return `age=${this.normaliseAgeRange(age)}:ethnicity=${this.normaliseEthnicityCode(
            ethnicity,
        )}:gender=${this.normaliseGender(gender)}`;
    }

    public normaliseAgeRange(age: string): string {
        return age.toLowerCase().trim();
    }

    public normaliseEthnicityCode(ethnicity: string): string {
        return ethnicity
            .replace(/\w+/g, function (word) {
                return word.charAt(0).toUpperCase() + word.substring(1);
            })
            .replace(/\s/g, '')
            .replace(/-/g, '')
            .replace(/\"/g, '');
    }

    public normaliseGender(gender: string): string {
        return gender.toLowerCase().trim();
    }
}
