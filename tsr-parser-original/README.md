# The Strategy Room Data Parser

Parsing tools written to process the data created by The Strategy Room so they can be stored in the database. Currently written locally for initial testing, but needs to be implemented on a Lambda or similar.

The tools in this repo perform 2 functions.

1. Convert the relevant files in to JSON (for easy processing)
2. Parse the relevant data into the document format used in the database, and save as a formatted .JSON

### Converting to JSON

Use `parse-csv-to-json.js` to do this. In the code set `folderName` to the name of the folder you want to parse. Make sure the folder only contains the csvs for a single session.

Run: `node parse-csv-to-json.js`

This will produce json files for the relevant files

### Parsing JSON files into an object for the databse

Use `tsr-parser-promise.js` to produce a single JSON object that represents the whole session (meta data, users + demographics and their responses)

To use, set `folderName` to the folder you have just converted to JSON files. Run the code and it will output `parsed.json` which is a single JSON object that represents the entire session. This can be inserted into DynamoDB.

Run: `node tsr-parser.promise.js`

### Potential improvements

- Producing JSON files and make a single object for the database could be done in a single file
- Spend a bit more time looking at the data for edge cases and inconsistencies - e.g. what do we do when there is no demographics file, do we just omit that data?
- Have it read an entire folder (e.g. a folder that has been synced from S3) and produce JSON objects
- The next extension from that is to automate putting these objects into the database, and hosting this code on AWS to run automatically.
