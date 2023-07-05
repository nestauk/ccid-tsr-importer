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

# find the file
SOURCE_PATH=s3/$S3_BUCKET/syndicateos-data/nesta/$PATH_KEY
echo "Examining $SOURCE_PATH..."
gfind $SOURCE_PATH -maxdepth 1 -type f -printf "%s %p\n" | sort -nr
LARGEST_FILE_DETAILS=$(gfind $SOURCE_PATH -maxdepth 1 -type f -printf "%s %p\n" | sort -nr | head -n 1)
SOURCE_FILE_PATH=$(echo $LARGEST_FILE_DETAILS | awk '{print $2}')
SOURCE_FILE=$(basename -- "$SOURCE_FILE_PATH")
echo "Largest file path: $SOURCE_FILE_PATH"
echo "Largest filename:  $SOURCE_FILE"

# transfer a working copy
cp $SOURCE_FILE_PATH working/$SOURCE_FILE

# unzip the file
echo "Unzipping $SOURCE_FILE..."
unzip -q -j working/$SOURCE_FILE -d working/files

# install all node packages
npm install --silent

# generate parsed data for dynamodb
echo "Parsing CSV to JSON..."
node parse-csv-to-json.js working/files
echo "Generating parsed JSON..."
node tsr-parser-promise.js working/files

PARSED_FILE=working/files/parsed.json
if test -f "$PARSED_FILE"; then
    echo "Created: $PARSED_FILE"
fi

OUTPUT_FILE=output/$PATH_KEY.parsed.json
cp -f $PARSED_FILE $OUTPUT_FILE
echo "Ready: $OUTPUT_FILE"
