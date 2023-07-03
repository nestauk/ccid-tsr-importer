# tsr-stats

A quick and dirty stats command line tool to see what's in our DynamoDB tables.

## Usage

Run the tool with `run-stats.sh`

```shell
$ ./run-stats.sh
```

Output summarises a number of vitals about data found in the tool.

```text
Session table: tsr-data-platform
Summary table: tsr-importer-app-SummaryTable-6TXVBV7ENKKK

Analysing sessions...
Sessions: 56
Total participants: 533

Analysing summaries...
Summaries: 266
Total participants: 533
```
