let fs = require('fs');
let fsPromise = require('fs/promises');

const folderName = '/Users/tomfeltwell/Code/tsr-parser/bucket-parsed/966990/1680526138699/';


const inputVotes = `${folderName}stage_text_input_votes.json`
const userDemog = `${folderName}user_demographics.json`;
const sliderVoteVotes = `${folderName}stage_slider_vote_votes.json`;
const stageTimings = `${folderName}stage_timings.json`;
const pollVotes = `${folderName}stage_slider_vote_results.csv`;  // This is parsed manually, CSV only
const checkboxVotes = `${folderName}stage_checkbox_votes.csv`;  // This is parsed manually, CSV only

async function readFiles(inputFile, demogFile, voteFile, timingFile, pollFile, checkboxFile) {
  try {
    const inputData = await fsPromise.readFile(inputFile, { encoding: 'ascii'});
    const demogData = await fsPromise.readFile(demogFile, { encoding: 'ascii' });
    const voteData = await fsPromise.readFile(voteFile, { encoding: 'ascii' });
    const timingData = await fsPromise.readFile(timingFile, { encoding: 'ascii' });
    const pollData = await fsPromise.readFile(pollFile, { encoding: 'ascii' });
    const checkboxData = await fsPromise.readFile(checkboxFile, { encoding: 'ascii' });

    // pollData is not parsed to JSON as this needs to be done manually
    return { 
      "input": JSON.parse(inputData),
      "demog": JSON.parse(demogData),
      "vote": JSON.parse(voteData),
      "timing": JSON.parse(timingData),
      "poll": pollData,
      "checkbox": checkboxData
    };
  } catch (err) {
    console.error(err);
  }
}

function extractPollResults(pollFile) {
  console.log('Parsing poll results');
  const pollData = [];
  // This is really hacky, as the embedded JSON object messes up CSV parsing because of the use of commas!
  const lines = pollFile.split('\n');

  // Ignore column heading row
  for (let i = 1; i < lines.length; i++) {
    const splitLine = lines[i].split('"{');
    let record = {
      "stage_id": "",
      "result": {},
      "vote_id": ""
    }

    if(splitLine.length > 1) {
      // [0] has the poll number and sess ID in
      const res0 = splitLine[0].split(',');
      record.stage_id = res0[1];

      // [1] has the object, and question number
      const res1 = splitLine[1].split('}"');
      record.vote_id = res1[1].replace(',','');
      const cleanedObj = res1[0].replaceAll('""', '"');
      record.result = eval('({' + cleanedObj + '})');
    }
    pollData.push(record);
  }
  return pollData;
}

function extractCheckboxResults(checkboxFile) {
  console.log('Parsing checkbox results');
  const checkboxData = [];
  // This is really hacky, as the embedded JSON object messes up CSV parsing because of the use of commas!
  const lines = checkboxFile.split('\n');

  // Ignore column heading row
  for (let i = 1; i < lines.length; i++) {
    // Split on the first "{ combination, as this is the only one in the line
    const splitLine = lines[i].split('"{');
    let record = {
      "cast_uuid": "",
      "stage_id": "",
      "result": {},
    }

    if(splitLine.length > 1) {
      // parse the first part of the line to extract the cast_uuid and stage_id
      const res0 = splitLine[0].split(',');
      record.cast_uuid = res0[1];
      record.stage_id = res0[2];

      // parse the second part of the line to extract the JSON object
      let substr = splitLine[1].split('}');
      let cleanedObj = substr[0].replaceAll('""', '"');
      record.result = eval('({' + cleanedObj + '})');
      // console.log('record:', record);
      checkboxData.push(record);
    } else {
      console.log('no entires on this line');
    }
  }
  return checkboxData;
}

readFiles(
  inputVotes,
  userDemog,
  sliderVoteVotes,
  stageTimings,
  pollVotes,
  checkboxVotes
).then( data => {
  console.log('Successfully read all files');
  // get LA name
  let session = {
    "session-id": data.input[0].session_id,
    "council": data.input[0].vote,
    "datetime": data.timing[0].end_time,
    "modules": {},
    "unfilterable_polls": []
  }

  let modules = {
    travel: false,
    heat: false,
    food: false
  };

  // Check the first letter of stage id, if it's t, h, or f[number]
  for (let a = 0; a < data.timing.length; a++) {
    switch (data.timing[a].stage_id.charAt(0)) {
      case 't':
        if (!modules.travel) modules.travel = true;
        break;
      case 'h':
        if (!modules.heat) modules.heat = true;
        break;
      case 'f':
        // code
        const regex = /[A-Za-z][0-9]+/i;
        if(data.timing[a].stage_id.search(regex) !== -1) {
          // it is food (not final question)
          modules.food = true;
        }
        break;
      default:
        break;
    }
  }
  session.modules = modules;
  
  // Manually parse the CSV for poll data
  session.unfilterable_polls = extractPollResults(data.poll);

  // Manually parse the CSV for checkbox data
  const all_checkbox_data = extractCheckboxResults(data.checkbox);
  
  // console.log(session);

  // Build list of participants
  let participants = [];
  for(let i = 0; i < data.demog.length; i++) {
    // Map the data to new object format
    let result = (({ 
      uuid,
      gender,
      age_range,
      ethnicity,
      first_half_postcode
    }) => ({ 
      uuid,
      "demographics": {gender, age_range, ethnicity, first_half_postcode},
      "responses": []
    }))(data.demog[i]);
    // console.log(result);
    participants.push(result);
  }
  // console.log(participants);

  // Iterate all participants and add their checkbox data
  for(let i = 0; i < participants.length; i++) {
    // Find all the checkbox data for this participant
    const checkbox_data = all_checkbox_data.filter(e => e.cast_uuid === participants[i].uuid);
    console.log('found this checkbox data for participant', participants[i].uuid, checkbox_data);
    // Add to the participant object in 'checkbox' field
    participants[i].checkbox = checkbox_data;
  }

  // Build list of votes
  for(let j = 0; j < data.vote.length; j++) {
    let result = (({ stage_id, vote, min_boundary, max_boundary, vote_id }) => ({ stage_id, vote, min_boundary, max_boundary, vote_id }))(data.vote[j]);

    // Search by cast_uuid, their uuid should be in there
    let idx = participants.findIndex(e => e.uuid === data.vote[j].cast_uuid);
    // If not found, something weird is going on (e.g. a vote without a demog record)
    // DEV NOTE: Demographics are mandatory so all records should have a demog record
    if(idx === -1) {
      console.error('Error: vote without demog record');
      console.log(data.vote[j]);
      continue;
    }

    // console.log(idx);
    // Find in participants, push to the [index].responses
    participants[idx].responses.push(result);
  }
  // console.log(participants[0].responses);

  session.participants = participants;

  fs.writeFile(`${folderName}parsed.json`, JSON.stringify(session), err => {
    if (err) {
      console.error(err);
    }
  });
});