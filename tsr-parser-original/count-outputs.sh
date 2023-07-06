#!/bin/bash

# init script
set -e 
set -o pipefail

usage() {
  cat << EOF
Counts all directories in the bucket, and all files in the output directory.

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

SESSIONS_PATH=s3/$S3_BUCKET/syndicateos-data/nesta/

BUCKET_SESSIONS=$(find $SESSIONS_PATH -type d | wc -l)
OUTPUTS=$(find output/ -type f | wc -l)

echo "$BUCKET_SESSIONS session dirs in: $SESSIONS_PATH"
echo "$OUTPUTS output files in: output/"
