#!/bin/bash

# init script
set -e 
set -o pipefail

echo "Are you sure you want to do this?"
echo "The full set of sessions includes testing data that should not be included."
echo "To proceed, remove the exit command from this script."
exit 1

S3_BUCKET=collective-simulation-tsr-data-uploads
SESSIONS_PATH=s3/$S3_BUCKET/syndicateos-data/nesta/*/

# init directories
mkdir -p s3/$S3_BUCKET
mkdir -p working
rm -r working/*
mkdir -p working/files
mkdir -p output

# erase all output files
echo "Erasing all output files..."
rm output/* || true
echo

# synchronise local data with bucket
echo "Synchronising data..."
./sync-s3.sh -b $S3_BUCKET
echo

echo "Preparing sessions..."
FAILURES=( )
for d in $SESSIONS_PATH ; do
    PATH_KEY=$(basename $d)

    echo "Preparing session: $PATH_KEY"
    echo

    ./generate-session-entry.sh -s $PATH_KEY -b $S3_BUCKET || FAILURES+=($PATH_KEY)
    echo
done

./count-outputs.sh -b $S3_BUCKET
echo

for FAIL in "${FAILURES[@]}"
do
     echo "$FAIL failed."
done
