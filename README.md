# ccid-tsr-importer

This repository contains tools for import and summarisation of data collected during The Strategy Room sessions.

> The Strategy Room is an immersive experience which uses facilitated discussion and social psychology to find out what non-experts really think about climate change policies. It’s a way for someone to walk in off the street and within 90 minutes, to imagine the benefits of a Net Zero future, and to have their say on how we get there.

- [The Strategy Room](https://www.nesta.org.uk/project/strategyroom) (Nesta)
- [The Strategy Room](https://fastfamiliar.com/research/the-strategy-room/) (case study, FastFamiliar)

## Repository contents

The [tsr-importer-app](tsr-importer-app/) directory contains a CloudFormation application with functions to import data uploaded to s3 from TSR sessions, summarise and store that data by demographic, and then present that data for consumption by [The Strategy Room data visualisation platform](https://strategyroom.uk).

See also: [nestauk/ccid-tsr-data-platform](https://github.com/nestauk/ccid-tsr-data-platform)

| directory                                                                                               | description                                                                         |
| ------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| [samples/collective-simulation-tsr-data-uploads_s3/](samples/collective-simulation-tsr-data-uploads_s3) | Sample s3 upload from a session                                                     |
| [samples/tsr-data-platform_dynamodb/](samples/tsr-data-platform_dynamodb/)                              | Sample dynamo db record for a session                                               |
| [samples/summary-endpoint/](samples/summary-endpoint/)                                                  | Sample data from the summary endpoint                                               |
| [tsr-importer-app/](tsr-importer-app/)                                                                  | CloudFormation app stack with import, summarisation, and summary endpoint functions |
| [tsr-parser-original/](tsr-parser-original/)                                                            | Parser and scripts to prepapre data and import it into our sessions DynamoDB table. |
| [tsr-question-map-generator/](tsr-question-map-generator/)                                              | Generates static dataset of questions for each of the dataviz tabs.                 |
| [tsr-stats/](tsr-stats/)                                                                                | Command-line analysis tool to see what's in our DynamoDB tables and S3 bucket.      |

## Resources

Key resources are:

- s3 bucket: [collective-simulation-tsr-data-uploads](https://s3.console.aws.amazon.com/s3/buckets/collective-simulation-tsr-data-uploads?region=eu-west-2&tab=properties)
- [tsr-importer-app](tsr-importer-app/) tables, functions, and API gateway

## Processes

### Import new session data

- Use scripts in the `tsr-parser-original` directory

  ```shell
  cd tsr-parser-original
  ./install-prerequisites.sh
  ```

- Learn the session id (6 digit numeric)
- Ensure that the session is in our s3 bucket
- Prepare an import file for the session

  ```shell
  ./generate-session-entry.sh --session <session-id> --bucket collective-simulation-tsr-data-uploads --update
  ```

- Ensure that `output/<session-id>.parsed.json` was created
- Upload the session to DynamoDB

  ```shell
  ./upload-session.sh --file output/<session-id>.parsed.json --table tsr-importer-app-SessionTable-O89MWVA1W5BQ
  ```

- Once the table has been updated, the summariser function will automatically run, and regenerate summaries of all the demographics in the summaries table.
- These summaries will be served.

You may encounter a small delay (worst case, up to 1h 15m) before the [strategyroom.uk](https:/strategyroom.uk) site updates. This is because the lambda functions that serve data to the site cache their data. Lambdas can live for up to about 45 mins before AWS recycles them. Meanwhile, the site itself caches unfiltered search data it received from the lambda for up to 30 mins.

### Review statistics

- Use scripts in the `tsr-stats` directory

  ```shell
  cd tsr-stats
  ./install-prerequisites.sh
  ./run-stats.sh
  ```

Statistics include

- the number of participants found in the s3 bucket, and in both the sessions and summaries tables
- the number of unique demographics identified in the summaries table
- the unique ethnicities, genders, and age ranges found in the s3 bucket and both tables

_If you encounter a discrepancy in the data, vs what you'd expect, this may help to determine which stage of the import process has introduced an error._

### Modify and redeploy the backend

- Work in the `tsr-import-app` directory
- Use the `dev-sync.sh` script to enable continuous sync with a dev stack of your choice

  ```
  ./dev-sync.sh tsr-importer-app-dev
  ```

- Modify the functions, or `template.yaml`, your changes will be automatically synchronised
- When ready, use the `prod-deploy.sh` script to sync your changes with the full stack

  ```
  ./prod-deploy.sh
  ```

NB. The summariser function will only run if triggered, ie.

- Any changes to the sessions table will trigger the summariser function
- You can also trigger it by [running a test](https://eu-west-2.console.aws.amazon.com/lambda/home?region=eu-west-2#/functions/tsr-importer-app-SummariserFunction-giC9VA6PguGg?tab=testing) in the console, with a blank input

NB. If you modify the way unique demographic codes are generated, you may need to delete everything in the summaries table, and then run the summariser function. This will ensure that old data is not retained. (If, however, the way demographic codes are created isn't changed, you'll be fine - these codes are used as the index on the table.)
