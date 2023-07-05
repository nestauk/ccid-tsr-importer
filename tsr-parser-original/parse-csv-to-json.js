let csvToJson = require('convert-csv-to-json');

// get the folder from parameter, not hard coded
// const folderName = '/Users/tomfeltwell/Code/tsr-parser/bucket-parsed/966990/1680526138699/';
var args = process.argv.slice(2);
let folderName = args[0];
if (!folderName.endsWith('/')) { folderName += '/'; }
if (folderName) {
    console.log(`Parsing directory: ${folderName}`);
} else {
    console.err('No folder name provided');
    process.exit(1);
}

// One template per file to be parsed
const inputName = `${folderName}stage_text_input_votes`
const demogName = `${folderName}user_demographics`;
const votesName = `${folderName}stage_slider_vote_votes`;
const stageName = `${folderName}stage_timings`;

// LA detail
try {
    csvToJson
    .fieldDelimiter(',')
    .generateJsonFileFromCsv(`${inputName}.csv`, `${inputName}.json`);
} catch (error) {
    console.log('LA details:', error);
}


// userDemographics
// DEV NOTE: Demographics are mandatory, so they shouldn't be missing
try {
    csvToJson
    .fieldDelimiter(',')
    .generateJsonFileFromCsv(`${demogName}.csv`, `${demogName}.json`);
} catch (error) {
    console.log('Demographics details:', error);
}


// voteVotes
try {
    csvToJson
        .fieldDelimiter(',')
        .generateJsonFileFromCsv(`${votesName}.csv`, `${votesName}.json`);
} catch (error) {
    console.log('Vote Votes:', error);
}

// Stage timings
try {
    csvToJson
    .fieldDelimiter(',')
    .generateJsonFileFromCsv(`${stageName}.csv`, `${stageName}.json`);
} catch (error) {
    console.log('Stage timings:', error);
}