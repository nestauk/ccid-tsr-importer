#!/bin/bash

# init script
set -e 
set -o pipefail

# init variables
PATH_KEY=$1
S3_BUCKET=collective-simulation-tsr-data-uploads

echo "S3 bucket: $S3_BUCKET"
echo "Session:   $PATH_KEY"

# init directories
mkdir -p s3/$S3_BUCKET
mkdir -p working
rm -r working/*
mkdir -p working/files
mkdir -p output

# prerequisites
brew install --quiet findutils

# sync all data
echo "Syncing data from S3..."
aws s3 sync s3://$S3_BUCKET s3/$S3_BUCKET --exclude "*" --include "*.zip"

# find the best candidate zip file to work from
SOURCE_PATH=s3/$S3_BUCKET/syndicateos-data/nesta/$PATH_KEY
echo "Examining $SOURCE_PATH..."
ls -1 $SOURCE_PATH
echo

# find the largest file
# gfind $SOURCE_PATH -maxdepth 1 -type f -printf "%s %p\n" | sort -nr
# LARGEST_FILE_DETAILS=$(gfind $SOURCE_PATH -maxdepth 1 -type f -printf "%s %p\n" | sort -nr | head -n 1)
# SOURCE_FILE_PATH=$(echo $LARGEST_FILE_DETAILS | awk '{print $2}')
# SOURCE_FILE=$(basename -- "$SOURCE_FILE_PATH")

if [ -z "${SOURCE_FILE}" ]; then
    ls -1 $SOURCE_PATH | (grep "\-repaired.zip$" || true) | sort -nr
    echo "Finding the file that ends with: -repaired.zip"
    CANDIDATE_FILES=$(ls -1 $SOURCE_PATH | (grep "\-repaired.zip$" || true) | sort -nr)
    BEST_FILE_DETAILS=$(echo "$CANDIDATE_FILES" | head -n 1)
    echo "$BEST_FILE_DETAILS"
    SOURCE_FILE=$BEST_FILE_DETAILS
    SOURCE_FILE_PATH=$SOURCE_PATH/$SOURCE_FILE
fi

if [ -z "${SOURCE_FILE}" ]; then
    echo "Finding the file that starts with: $PATH_KEY"
    CANDIDATE_FILES=$(ls -1 $SOURCE_PATH | (grep "^$PATH_KEY" || true) | sort -nr)
    BEST_FILE_DETAILS=$(echo "$CANDIDATE_FILES" | head -n 1)
    echo "$BEST_FILE_DETAILS"
    SOURCE_FILE=$BEST_FILE_DETAILS
    SOURCE_FILE_PATH=$SOURCE_PATH/$SOURCE_FILE
fi

if [ -z "${SOURCE_FILE}" ]; then
    echo "Finding the file with the most recent timestamp..."
    CANDIDATE_FILES=$(ls -1 $SOURCE_PATH | (grep "csv" || true) | sort -nr)
    BEST_FILE_DETAILS=$(echo "$CANDIDATE_FILES" | head -n 1)
    echo "$BEST_FILE_DETAILS"
    SOURCE_FILE=$BEST_FILE_DETAILS
    SOURCE_FILE_PATH=$SOURCE_PATH/$SOURCE_FILE
fi
echo

echo "Selected file: $SOURCE_FILE_PATH"
echo

# transfer a working copy
cp $SOURCE_FILE_PATH working/$SOURCE_FILE

# unzip the file
echo "Unzipping $SOURCE_FILE..."
unzip -q -j working/$SOURCE_FILE -d working/files
echo

# install all node packages
npm install --silent

# generate parsed data for dynamodb
echo "Parsing CSV to JSON..."
node parse-csv-to-json.js working/files
echo

echo "Generating parsed JSON..."
node tsr-parser-promise.js working/files
echo

PARSED_FILE=working/files/parsed.json
if test -f "$PARSED_FILE"; then
    echo "Created: $PARSED_FILE"
fi

OUTPUT_FILE=output/$PATH_KEY.parsed.json
cp -f $PARSED_FILE $OUTPUT_FILE
echo "Ready: $OUTPUT_FILE"
