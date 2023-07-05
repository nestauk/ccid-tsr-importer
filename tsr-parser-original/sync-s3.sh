#!/bin/bash

S3_BUCKET=collective-simulation-tsr-data-uploads

# sync all data
echo "Syncing data from S3..."
aws s3 sync s3://$S3_BUCKET s3/$S3_BUCKET --exclude "*" --include "*.zip"
