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
................................................................
1 errors
- 380103: InvalidOperationException: Sequence contains no matching element

S3 sessions: 64
Total participants: 622
Unique councils: 13
Barnsley Metropolitan Borough Council session dates: 2023-03-29, 2023-03-30, 2023-03-30, 2023-03-30, 2023-03-29
Barnsley Metropolitan Borough Council session keys: 101455, 193868, 225497, 464359, 847340
Reading Borough Council session dates: 2023-03-21, 2023-03-22, 2023-03-21, 2023-03-22, 2023-03-21, 2023-03-22
Reading Borough Council session keys: 103954, 378650, 525529, 630960, 721202, 934788
Sandwell Metropolitan Borough Council session dates: 2023-03-17, 2023-03-17, 2023-03-15, 2023-03-15, 2023-03-16, 2023-03-16, 2023-03-16, 2023-03-17, 2023-03-15
Sandwell Metropolitan Borough Council session keys: 108939, 388301, 504587, 518641, 536946, 552187, 636411, 795899, 871991
London Borough of Lambeth session dates: 2023-02-23, 2023-03-06, 2023-03-10, 2023-02-24, 2023-02-08, 2023-02-24, 2023-02-23, 2023-02-08, 2023-03-10, 2023-03-06
London Borough of Lambeth session keys: 111215, 369479, 391133, 407144, 512506, 611543, 696052, 883864, 908247, 963024
Southend-on-Sea Borough Council session dates: 2023-01-31, 2023-01-31, 2023-01-30, 2023-02-20, 2023-01-30, 2023-02-20, 2023-01-30, 2023-01-31
Southend-on-Sea Borough Council session keys: 119898, 187495, 188871, 212039, 239721, 544690, 676816, 698196
Cornwall Council (Unitary) session dates: 2023-02-03, 2023-02-04, 2023-02-04, 2023-02-03, 2023-02-03, 2023-02-04
Cornwall Council (Unitary) session keys: 141491, 173653, 528464, 565725, 684399, 959537
London Borough of Wandsworth session dates: 2023-03-06, 2023-02-25, 2023-03-09, 2023-02-25
London Borough of Wandsworth session keys: 174622, 413673, 689852, 793121
London Borough of Barnet session dates: 2023-03-05, 2023-03-05, 2023-03-05
London Borough of Barnet session keys: 185524, 217445, 728338
Luton Borough Council session dates: 2023-03-01, 2023-03-01, 2023-03-01
Luton Borough Council session keys: 279819, 538479, 598115
London Borough of Merton session dates: 2023-02-16, 2023-02-18, 2023-02-16
London Borough of Merton session keys: 340608, 393798, 478594
London Borough of Bromley session dates: 2023-03-28, 2023-03-28, 2023-03-28
London Borough of Bromley session keys: 445930, 468486, 615553
Medway Council session dates: 2023-03-03, 2023-03-03, 2023-03-03
Medway Council session keys: 477475, 834156, 966990
Christchurch Borough Council session dates: 2023-01-20
Christchurch Borough Council session keys: 760987

Analysing imported sessions...
Sessions: 56
Total participants: 533

Analysing summaries...
Demographic summaries: 266
Total participants: 533

Storing analysis...
Done.
```
