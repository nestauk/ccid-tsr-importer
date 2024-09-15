#!/bin/bash

set -e
set -o pipefail

PATH=output/workshops/suffolk_county_council
RULES=export/workshop-to-csv.json
TOOL=export/JsonToSmartCsv

echo "Converting workshop output to CSV..."

# loop through files in $PATH
for FILE in $PATH/*.json; do
  FILENAME=$(/usr/bin/basename $FILE)
  OUTPUT_FILENAME="${FILENAME%.*}"
  echo "Converting $FILE to: $OUTPUT_FILENAME.csv..."
  $TOOL --columns $RULES --source $FILE --target $PATH/$OUTPUT_FILENAME.csv
done
