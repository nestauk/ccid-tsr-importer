#!/bin/bash

# init script
set -e 
set -o pipefail

SESSION_TABLE=tsr-importer-app-SessionTable-O89MWVA1W5BQ

echo "Are you sure you want to do this?"
echo "The full set of sessions includes testing data that should not be included."
echo "To proceed, remove the exit command from this script."
exit 1

echo "Uploading files..."
for SESSION_FILE in output/*; do
    ./upload-session.sh -f $SESSION_FILE -t $SESSION_TABLE
done
