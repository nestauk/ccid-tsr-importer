# tsr-stats

A quick and dirty stats command line tool to see what's in our DynamoDB tables.

## Generate statistics

Run the tool with `run-stats.sh`

```shell
$ ./run-stats.sh
```

- The printed output summarises a number of stats about data found in:
  - **Sessions in S3** - the local synchronised copy of the S3 bucket
  - **Imported sessions** - the sessions uploaded to the import DynamoDb table
  - **Summaries** - the demographic summaries stored in their DynamoDb table
  - **Endpoint** - data made available through the endpoint for the `*` demographic
- The `output/` directory contains records for analysis.
- The `output/workshops/all-workshops.json` file contains a list of all workshops, with question totals
- `output/workshops/<local-authority>/*.json` represents each individual workshop, with question totals

## Example output

```text
S3 bucket:                          collective-simulation-tsr-data-uploads
Session table:                      tsr-importer-app-SessionTable-O89MWVA1W5BQ
Summary table:                      tsr-importer-app-SummaryTable-6TXVBV7ENKKK
Data endpoint:                      https://wx2igcevv7.execute-api.eu-west-2.amazonaws.com/Prod/summary
Individual workshops summary table: tsr-importer-app-IndividualWorkshopSummaryTable-RLLSZJ05LT3X

Analysing sessions in S3...
.......................................................................................................................
0 errors

S3 sessions: 119
Total participants: 955
Unique councils: 21
Unique age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Unique ethnicities: "African", "Any other Asian background", "Any other Black, Black British, or Caribbean background", "Any other ethnic group", "Any other Mixed or multiple ethnic background", "Any other White background", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English, Welsh, Scottish, Northern Irish or British", "Gypsy or Irish Traveller", "Indian", "Irish", "Pakistani", "prefer-not-to-answer", "Roma", "White and Asian", "White and Black African", "White and Black Caribbean"
Unique genders: "female", "male", "non-binary", "not-disclosed"

Analysing imported sessions...
Sessions: 93
Total participants: 843
Unique age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Unique ethnicities: "African", "Any other Asian background", "Any other Black, Black British, or Caribbean background", "Any other ethnic group", "Any other Mixed or multiple ethnic background", "Any other White background", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English, Welsh, Scottish, Northern Irish or British", "Gypsy or Irish Traveller", "Indian", "Irish", "Pakistani", "prefer-not-to-answer", "Roma", "White and Asian", "White and Black African", "White and Black Caribbean"
Unique genders: "female", "male", "non-binary", "not-disclosed"

Analysing summaries...
Demographic summaries: 397
Total participants: 845
Unique councils: "BarnsleyMetropolitanBoroughCouncil", "CalderdaleMetropolitanBoroughCouncil", "CanterburyCityCouncil", "CheshireWestAndChesterCouncil", "CornwallCouncil(Unitary)", "HaltonBoroughCouncil", "LondonBoroughOfBarnet", "LondonBoroughOfBromley", "LondonBoroughOfLambeth", "LondonBoroughOfMerton", "LondonBoroughOfWandsworth", "LutonBoroughCouncil", "MedwayCouncil", "ReadingBoroughCouncil", "SandwellMetropolitanBoroughCouncil", "SouthendOnSeaBoroughCouncil", "SuffolkCountyCouncil"
Unique age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Unique ethnicities: "African", "AnyOtherAsianBackground", "AnyOtherBlack,BlackBritish,OrCaribbeanBackground", "AnyOtherEthnicGroup", "AnyOtherMixedOrMultipleEthnicBackground", "AnyOtherWhiteBackground", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English,Welsh,Scottish,NorthernIrishOrBritish", "GypsyOrIrishTraveller", "Indian", "Irish", "not-disclosed", "Pakistani", "prefer-not-to-answer", "Roma", "WhiteAndAsian", "WhiteAndBlackAfrican", "WhiteAndBlackCaribbean"
Unique genders: "female", "male", "non-binary", "not-disclosed"

Analysing endpoint...
Participants: 845
Councils: "BarnsleyMetropolitanBoroughCouncil", "CalderdaleMetropolitanBoroughCouncil", "CanterburyCityCouncil", "CheshireWestAndChesterCouncil", "CornwallCouncil(Unitary)", "HaltonBoroughCouncil", "LondonBoroughOfBarnet", "LondonBoroughOfBromley", "LondonBoroughOfLambeth", "LondonBoroughOfMerton", "LondonBoroughOfWandsworth", "LutonBoroughCouncil", "MedwayCouncil", "ReadingBoroughCouncil", "SandwellMetropolitanBoroughCouncil", "SouthendOnSeaBoroughCouncil", "SuffolkCountyCouncil"
Age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Ethnicities: "African", "AnyOtherAsianBackground", "AnyOtherBlack,BlackBritish,OrCaribbeanBackground", "AnyOtherEthnicGroup", "AnyOtherMixedOrMultipleEthnicBackground", "AnyOtherWhiteBackground", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English,Welsh,Scottish,NorthernIrishOrBritish", "GypsyOrIrishTraveller", "Indian", "Irish", "not-disclosed", "Pakistani", "prefer-not-to-answer", "Roma", "WhiteAndAsian", "WhiteAndBlackAfrican", "WhiteAndBlackCaribbean"
Genders: "female", "male", "non-binary", "not-disclosed"

Analysing individual workshop summaries...
Storing analysis...
Done.
```

## Converting individual json workshop summaries

Individual workshop summaries are available as json files, but where requested, these are more easily consumed as CSV. We use [`JsonToSmartCSV`](https://github.com/instantiator/json-to-smart-csv) to convert them.

A copy of the Mac OS binary for this, and the conversion definition file is in the `tsr-stats/export` directory.

The `workshop-output-to-csv.sh` file will convert these JSON files to CSV.

Output CSV files are placed in the same directory as the source JSON files.
