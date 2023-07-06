#!/bin/bash

# init script
set -e 
set -o pipefail

S3_BUCKET=collective-simulation-tsr-data-uploads
SESSIONS_PATH=s3/$S3_BUCKET/syndicateos-data/nesta/*/

# init directories
mkdir -p s3/$S3_BUCKET
mkdir -p working
rm -r working/*
mkdir -p working/files
mkdir -p output

# synchronise local data with bucket
./sync-s3.sh -b $S3_BUCKET

for d in $SESSIONS_PATH ; do
    PATH_KEY=$(basename $d)

    echo "Preparing session: $d"
    echo

    ./generate-session-entry.sh -s $PATH_KEY -b $S3_BUCKET
    echo
done

./count-outputs -b $S3_BUCKET
