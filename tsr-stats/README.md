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
Session table: tsr-importer-app-SessionTable-O89MWVA1W5BQ
Summary table: tsr-importer-app-SummaryTable-6TXVBV7ENKKK

Analysing sessions in S3...
..................................................................
0 errors

S3 sessions: 66
Total participants: 638
Unique councils: 12
Unique age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Unique ethnicities: "African", "Any other Asian background", "Any other Black, Black British, or Caribbean background", "Any other ethnic group", "Any other Mixed or multiple ethnic background", "Any other White background", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English, Welsh, Scottish, Northern Irish or British", "Indian", "Irish", "Pakistani", "prefer-not-to-answer", "Roma", "White and Asian", "White and Black African", "White and Black Caribbean"
Unique genders: "female", "male", "non-binary", "not-disclosed"

Analysing imported sessions...
Sessions: 64
Total participants: 620
Unique age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Unique ethnicities: "African", "Any other Asian background", "Any other Black, Black British, or Caribbean background", "Any other ethnic group", "Any other Mixed or multiple ethnic background", "Any other White background", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English, Welsh, Scottish, Northern Irish or British", "Indian", "Irish", "Pakistani", "prefer-not-to-answer", "Roma", "White and Asian", "White and Black African", "White and Black Caribbean"
Unique genders: "female", "male", "non-binary", "not-disclosed"

Analysing summaries...
Demographic summaries: 416
Total participants: 977
Unique age ranges: "14-17", "18-25", "26-35", "36-45", "46-55", "56-65", "66-75", "76+", "not-disclosed"
Unique ethnicities: "African", "AnyOtherAsianBackground", "AnyOtherBlack,BlackBritish,OrCaribbeanBackground", "AnyOtherBlack", "AnyOtherEthnicGroup", "AnyOtherMixedOrMultipleEthnicBackground", "AnyOtherWhiteBackground", "Arab", "Bangladeshi", "Caribbean", "Chinese", "English,Welsh,Scottish,NorthernIrishOrBritish", "English", "Indian", "Irish", "not-disclosed", "Pakistani", "prefer-not-to-answer", "Roma", "WhiteAndAsian", "WhiteAndBlackAfrican", "WhiteAndBlackCaribbean"
Unique genders: "female", "male", "non-binary", "not-disclosed"

Storing analysis...
Done.
```
