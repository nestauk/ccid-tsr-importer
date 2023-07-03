# tsr-stats

A quick and dirty stats command line tool to see what's in our DynamoDB tables.

## Usage

Run the tool with `run-stats.sh`

```shell
$ ./run-stats.sh
```

Output summarises a number of vitals about data found in the tool.

```text
S3 bucket:     collective-simulation-tsr-data-uploads
Session table: tsr-data-platform
Summary table: tsr-importer-app-SummaryTable-6TXVBV7ENKKK

Analysing sessions in S3...
.................................................................
S3 sessions: 65
Total participants: 631

Analysing imported sessions...
Sessions: 56
Total participants: 533

Analysing summaries...
Demographic summaries: 266
Total participants: 533
```
