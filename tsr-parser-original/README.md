# tsr-parser-original

## Scripts

These scripts assume that `bash` and `brew` are available. ([Homebrew](https://brew.sh/) is a Mac OS package manager.)

| Script                            | purpose                                                               |
| --------------------------------- | --------------------------------------------------------------------- |
| `install-prerequisites.sh`        | Some basic prerequisites.                                             |
| `sync-s3.sh`                      | Synchronises an s3 bucket into the `s3` directory                     |
| `generate-session-entry.sh`       | Prepare an import for a specific session in a specific bucket         |
| `generate-all-session-entries.sh` | Prepare all session entries for everything in the TSR s3 bucket       |
| `upload-session.sh`               | Upload a specific session file to a specific dynamodb table           |
| `upload-all-sessions.sh`          | Upload all session files found in `output/` to the TSR sessions table |

These are supported by some original javascript scripts that can manipulate the data itself:

| Script                  | purpose                                                                                  |
| ----------------------- | ---------------------------------------------------------------------------------------- |
| `parse-csv-to-json.js`  | Javascript code to generate JSON from CSV files for a specific session                   |
| `tsr-parser-promise.js` | Generate DynamoDB-marshalled session data from JSON and CSV files for a specific session |

## Resources

| Resource                                     | description        |
| -------------------------------------------- | ------------------ |
| `collective-simulation-tsr-data-uploads`     | TSR s3 bucket      |
| `tsr-importer-app-SessionTable-O89MWVA1W5BQ` | TSR sessions table |

## Processes

Use these scripts to sync s3 files, prepare session data and upload it to our DynamoDB sessions table.

Steps are documented in the root [README](../README.md).
