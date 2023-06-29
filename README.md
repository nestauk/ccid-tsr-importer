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
| [tsr-importer-app/](tsr-importer-app/)                                                                  | CloudFormation app stack with import, summarisation, and summary endpoint functions |
| [samples/collective-simulation-tsr-data-uploads_s3/](samples/collective-simulation-tsr-data-uploads_s3) | Sample s3 upload from a session                                                     |
| [samples/tsr-data-platform_dynamodb/](samples/tsr-data-platform_dynamodb/)                              | Sample dynamo db record for a session                                               |
| [samples/summary-endpoint/](samples/summary-endpoint/)                                                  | Sample data from the summary endpoint                                               |
| [tsr-parser-original/](tsr-parser-original/)                                                            | Original parser that prepapred data for import.                                     |
| [static-question-map-generator/](static-question-map-generator/)                                        | Generates static dataset of questions for each of the dataviz tabs.                 |
