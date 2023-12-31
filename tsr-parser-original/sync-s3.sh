#!/bin/bash

# init script
set -e 
set -o pipefail

usage() {
  cat << EOF
Synchronises all local data in the s3 folder with the given bucket.

Options:
    -b <bucket>      --bucket <bucket>       Work from the named bucket (optional)
    -h               --help                  Prints this help message and exits

EOF
}

# parameters
while [ -n "$1" ]; do
  case $1 in
  -b | --bucket)
    shift
    S3_BUCKET=$1
    ;;
  -h | --help)
    usage
    exit 0
    ;;
  *)
    echo -e "Unknown option $1...\n"
    usage
    exit 1
    ;;
  esac
  shift
done

if [ -z "$S3_BUCKET" ]; then
  echo "Please provide a bucket name."
  echo
  usage
  exit 1
fi

# sync all data
echo "Syncing data from S3 bucket: $S3_BUCKET..."
aws s3 sync s3://$S3_BUCKET s3/$S3_BUCKET --exclude "*" --include "*.zip"
