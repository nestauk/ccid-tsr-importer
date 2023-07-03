# tsr-question-map-generator

This tool builds `question-map.json` - a file describing the various questions and policies to display on each section of the data visualisation site.

## Data

- Data is drawn from [2023-06 TSR question map](https://docs.google.com/spreadsheets/d/1ETpYKrSqjuNf0tYxK0cPeHdp6cM14ThOrFI6-VgAvmU/edit?usp=sharing) (Google Sheet)
- The sheet is published as CSV, and there is an endpoint for each tab which this application consumes.
- URLs are configured in: `run-generator.sh`

| CSV data  | URL                                                                                                                                                                      |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| policies  | https://docs.google.com/spreadsheets/d/e/2PACX-1vT2EEl9ReSBsQp7gErWp7Mfq-Xc41qolOAFjBw6DevQKoLrTh7J2GiB2-OPVGhWG9a80XMGezmncfbU/pub?gid=216103307&single=true&output=csv |
| questions | https://docs.google.com/spreadsheets/d/e/2PACX-1vT2EEl9ReSBsQp7gErWp7Mfq-Xc41qolOAFjBw6DevQKoLrTh7J2GiB2-OPVGhWG9a80XMGezmncfbU/pub?gid=0&single=true&output=csv         |

## Output

The output is: `question-map.json`

## Prerequisites

- .NET SDK 7 or above

## Run the tool

```
./run-generator.sh
```
