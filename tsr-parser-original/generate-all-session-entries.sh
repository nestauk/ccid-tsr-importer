#!/bin/bash

# init script
set -e 
set -o pipefail

BUCKET_NAME=collective-simulation-tsr-data-uploads
SESSIONS_PATH=s3/$BUCKET_NAME/syndicateos-data/nesta/*/

# synchronise local data with bucket
./sync-s3.sh -b $BUCKET_NAME

for d in $SESSIONS_PATH ; do
    PATH_KEY=$(basename $d)

    echo "Preparing session: $d"
    echo

    ./generate-session-entry.sh -s $PATH_KEY -b $BUCKET_NAME
    echo
done

./count-outputs -b $BUCKET_NAME
