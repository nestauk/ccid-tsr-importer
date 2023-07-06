#!/bin/bash

# init script
set -e 
set -o pipefail

SESSION_TABLE=tsr-importer-app-SessionTable-O89MWVA1W5BQ

echo "Uploading files..."
for SESSION_FILE in output/*; do
    ./upload-session.sh -f $SESSION_FILE -t $SESSION_TABLE
done
