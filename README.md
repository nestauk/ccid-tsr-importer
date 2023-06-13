# ccid-tsr-importer

Importer for The Strategy Room data.

## Data bucket

Data is stored in an S3 bucket and then manually imported into DynamoDb.

- Bucket: [collective-simulation-tsr-data-uploads](https://s3.console.aws.amazon.com/s3/buckets/collective-simulation-tsr-data-uploads?region=eu-west-2&tab=properties)
- ARN: `arn:aws:s3:::collective-simulation-tsr-data-uploads`

## Parser

The parser is code that can process data from The Strategy Room into a format suitable for storage in our DynamoDb instance.

## Data structure

Data is stored in the bucket following this scheme:

- `/syndicateos-data/nesta/<6-digit-id>/<13-digit-timestamp>.zip`
- `/syndicateos-data/nesta/<6-digit-id>/<13-digit-timestamp>.zip`
- `/syndicateos-data/nesta/<6-digit-id>/<13-digit-timestamp>.zip`
- `/syndicateos-data/nesta/<6-digit-id>/<6-digit-timestamp>-<13-digit-id>.zip`

**Use the file that's prefixed with `<6-digit-id>`.** It should also be the most recent and largest. The others are snapshots taken during a session.

## CSV files in the zips

- `stage_text_input_votes.csv` contains emails and consent
- `stage_text_input_results.csv`
- `stage_slider_vote_votes.csv` contains post-survey items
- `stage_slider_vote_results.csv` contains aggregated post-survey items
