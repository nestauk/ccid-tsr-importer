# tsr-stats

A quick and dirty stats command line tool to see what's in our DynamoDB tables.

## Usage

Run the tool with `run-stats.sh`

```shell
$ ./run-stats.sh
```

- Output summarises a number of vitals about data found in the tool.
- `output/` directory contains records for analysis.

```text
S3 bucket:     collective-simulation-tsr-data-uploads
Session table: tsr-data-platform
Summary table: tsr-importer-app-SummaryTable-6TXVBV7ENKKK

Analysing sessions in S3...
................................................................
1 errors
- 380103: FileNotFoundException: (syndicateos-data/nesta/380103/380103-1680526138435.zip, 11373) does not contain stage_text_input_votes.csv

S3 sessions: 64
Total participants: 622
Unique councils: 13

Analysing imported sessions...
Sessions: 56
Total participants: 533

Analysing summaries...
Demographic summaries: 266
Total participants: 533

Storing analysis...
Done.
```
