# tsr-importer-app

This application manages import, summarisation and sharing of data from The Strategy Room.

It is managed as a [SAM](https://aws.amazon.com/serverless/sam/) application, hosted as a [CloudFormation stack](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/stacks.html) on AWS.

The stack runs in the AWS account for the CCID, in `eu-west-2`, and is named: `tsr-importer-app`

## Scripts

| Script                | Description                                                   |
| --------------------- | ------------------------------------------------------------- |
| `dev-sync.sh <stack>` | Synchronise the application with any dev CloudFormation stack |
| `prod-deploy.sh`      | Deploy to the prod `tsr-importer-app` CloudFormation stack    |

## Resources

| Resource             | Title                                                                                                                                                                                                                                                                                                                              | ARN                                                                                                         |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| S3 bucket            | [collective-simulation-tsr-data-uploads](https://s3.console.aws.amazon.com/s3/buckets/collective-simulation-tsr-data-uploads?region=eu-west-2&tab=properties)                                                                                                                                                                      | `arn:aws:s3:::collective-simulation-tsr-data-uploads`                                                       |
| CloudFormation stack | [tsr-importer-app](https://eu-west-2.console.aws.amazon.com/cloudformation/home?region=eu-west-2#/stacks/stackinfo?filteringText=&filteringStatus=active&viewNested=true&stackId=arn%3Aaws%3Acloudformation%3Aeu-west-2%3A251687087743%3Astack%2Ftsr-importer-app%2Ff20d84a0-0ab3-11ee-a92a-0ae36c4f8bfe) (in `eu-west-2`, London) | `arn:aws:cloudformation:eu-west-2:251687087743:stack/tsr-importer-app/f20d84a0-0ab3-11ee-a92a-0ae36c4f8bfe` |

The stack itself contains a number of resources, including:

| Resource                    | id                                                                                                                                                                                             |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Sessions table              | [tsr-importer-app-SessionTable-O89MWVA1W5BQ](https://eu-west-2.console.aws.amazon.com/dynamodb/home?region=eu-west-2#tables:selected=tsr-importer-app-SessionTable-O89MWVA1W5BQ)               |
| Summaries table             | [tsr-importer-app-SummaryTable-6TXVBV7ENKKK](https://eu-west-2.console.aws.amazon.com/dynamodb/home?region=eu-west-2#tables:selected=tsr-importer-app-SummaryTable-6TXVBV7ENKKK)               |
| Summariser lambda function  | [tsr-importer-app-SummariserFunction-giC9VA6PguGg](https://eu-west-2.console.aws.amazon.com/lambda/home?region=eu-west-2#functions/tsr-importer-app-SummariserFunction-giC9VA6PguGg)           |
| Summaries endpoint function | [tsr-importer-app-SummaryEndpointFunction-r3b029xddnZo](https://eu-west-2.console.aws.amazon.com/lambda/home?region=eu-west-2#functions/tsr-importer-app-SummaryEndpointFunction-r3b029xddnZo) |

## Functions

The application features 3 functions:

- Application lambda functions, tables, and API are defined in: `template.yaml`
- Each function is written in [TypeScript](https://www.typescriptlang.org/), and has its own directory.

| Function                  | Directory               | Description                                                                          |
| ------------------------- | ----------------------- | ------------------------------------------------------------------------------------ |
| `ImporterFunction`        | `tsr-importer/`         | **Under construction.** Imports data from a known S3 bucket into the data table.     |
| `SummariserFunction`      | `tsr-summariser/`       | Summarises data found in the data table into a summaries table for easy consumption. |
| `SummaryEndpointFunction` | `tsr-summary-endpoint/` | Retrieves data per demographic query from the summaries table.                       |

There are a number of data tables. Their names are provided as parameters to `template.yaml`

| Table                   | Default value       | Description                                                                     |
| ----------------------- | ------------------- | ------------------------------------------------------------------------------- |
| `DataPlatformTableName` | `tsr-data-platform` | Data imported from the S3 bucket from The Strategy Room sessions.               |
| `SummaryTableName`      | `tsr-summary`       | Demographic aggregated summaries, regenerated each time the data table updates. |
| `QuestionsTableName`    | `tsr-questions`     | Information about all The Strategy Room questions.                              |

## Importer

❗️ `Under construction`

For now, there are scripts and supporting documentation in the [tsr-parser-original](../tsr-parser-original/) directory and root [README](../README.md) explaining how to manually import session data.

### Data bucket

Data is stored in an S3 bucket and then manually imported into DynamoDb.

- Bucket: [collective-simulation-tsr-data-uploads](https://s3.console.aws.amazon.com/s3/buckets/collective-simulation-tsr-data-uploads?region=eu-west-2&tab=properties)
- ARN: `arn:aws:s3:::collective-simulation-tsr-data-uploads`

### Parser

The parser is code that can process data from The Strategy Room into a format suitable for storage in our DynamoDb instance.

### Data structure

Data is stored in the bucket following this scheme:

- `/syndicateos-data/nesta/<6-digit-id>/<13-digit-timestamp>.zip`
- `/syndicateos-data/nesta/<6-digit-id>/<13-digit-timestamp>.zip`
- `/syndicateos-data/nesta/<6-digit-id>/<13-digit-timestamp>.zip`
- `/syndicateos-data/nesta/<6-digit-id>/<6-digit-timestamp>-<13-digit-id>.zip`

**Use the file that's prefixed with `<6-digit-id>`.** It should also be the most recent and largest. The others are snapshots taken during a session.

### CSV files in the zips

- `stage_text_input_votes.csv` contains emails and consent
- `stage_text_input_results.csv`
- `stage_slider_vote_votes.csv` contains post-survey items
- `stage_slider_vote_results.csv` contains aggregated post-survey items

## Summariser

Generates summaries from the data table, and stores them in the summary table.

Data is aggregated by demographic code. A demographic code looks like: `council=*:age=*:ethnicity=*:gender=*` (where `*` is a wildcard)

Each participant from each session has demographic data associated with them. Each is assigned a demographic code, and then voting information is totalled for each unique demographic code found.

- [DataStructures.ts](https://github.com/nestauk/ccid-tsr-importer/blob/main/tsr-importer-app/tsr-summariser/DataStructures.ts) - defines the `Summary` data structure
- [SessionScanner.ts](https://github.com/nestauk/ccid-tsr-importer/blob/main/tsr-importer-app/tsr-summariser/SessionScanner.ts) - scans the session data, with functions to collate unique demographics and summaries per demographic

The summariser function is triggered by the data table's DynamoDb Stream. This means that any changes to the data table will be automatically reflected in the summary table.

## Summaries endpoint

Provide some or none of query string parameters `council`, `gender`, `ethnicity`, and `age` to search.

_NB. `council` is not really a demographic, but these are really values to filter by. During summarisation by the summariser function, each participant is attributed to the council for the session they participated in._

Notes

- Each of these should be a value you already know
- Leave out any demographic parameters to match all values in their category
- You can provide the concil and ethnic code as you have it, and it'll be normalised (as best as possible)
- To help with debugging, all demographics known to the system are in the `.all_demographics` property of the output from this endpoint

Examples:

- `<endpoint>/Prod/summary?gender=male`
- `<endpoint>/Prod/summary?ethnicity=Bangladeshi`
- `<endpoint>/Prod/summary?age=18-25`
- `<endpoint>/Prod/summary?council=LondonBoroughOfLambeth`

Combine these filters as you'd expect:

- `<endpoint>/Prod/summary?age=18-25&gender=male`
- `<endpoint>/Prod/summary?age=18-25&gender=male&council=LondonBoroughOfLambeth`

### Successful output

- `.success` - `true` if the search succeeded (even if 0 results)
- `.duration_ms` - how long it took to serve the query
- `.message` - quick summary of the action taken
- `.query` - the query parameters found (or the wildcard `*` substituted if any parameter was not provided)
- `.all_demographics` - all possible demographics and unique demographic codes found in the data
- `.included_demographics.found` - all demographic codes found in the data to be included in the search results
- `.included_demographics.not_found` - all demographic codes that were searched for inclusion in the results, but not found in the data
- `.summary` - your actual results
  - `.id` - demographic code generated from the query
  - `.demographic` - same as id
  - `.created` - time of creation of these results (in ISO format)
  - `.timestamp` - timestamp of creation of these results
  - `.participants` - participants that fitted the demographic query (NB. not all participants responded to every vote - so there's also a participant count for each vote)
  - `.questionTotals` - a map of vote_id to an object containing details about the vote summed across all the demographically matched participants
    - `[vote_id]`
      - `.demographic_code` - demographic code for which this question's results were filtered (same as `.summary.demographic`)
      - `.vote_id` - same as the `[vote_id]` key for this question
      - `.stage_id` - stage id for this question
      - `.max_boundary` - highest vote value for this question
      - `.min_boundary` - lowest vote value for this question
      - `.participants` - the number of participants from the demographic who took part in this vote
      - `.totals` - a map of the choices and counts (eg. `"1": 5` indicates that 5 people voted `1`)

### Unsuccessful output

- `.duration_ms` - how long it took to respond
- `.success` - `false`
- `.message` - short description (not very helpful - the logs from the lambda function that supports this endpoint are more helpful)
- `.query` - details of the query parameters found
- `.error` - error information if available
