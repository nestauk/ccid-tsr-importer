#!/bin/bash

set -e
set -o pipefail

PATH=output/workshops
RULES=export/workshop-to-csv.json
TOOL=export/JsonToSmartCsv

echo "Converting workshop output to CSV..."

# loop through files in $PATH
for DIR in $PATH/*; do
  if [ -d $DIR ]; then
    for FILE in $DIR/*.json; do
      FILENAME=$(/usr/bin/basename $FILE)
      OUTPUT_FILENAME="${FILENAME%.*}"
      echo "Converting $FILE to: $OUTPUT_FILENAME.csv..."
      $TOOL --columns $RULES --source $FILE --target $DIR/$OUTPUT_FILENAME.csv
    done
  fi
done
