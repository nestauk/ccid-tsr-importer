#!/bin/bash

# init script
set -e 
set -o pipefail

usage() {
  cat << EOF
Uploads a given session file to the given dynamodb table.

Options:
    -f <filename>    --file <filename>       The file to upload (required)
    -t <table-name>  --table <table-name>    The dynamodb table to upload to (required)
    -h               --help                  Prints this help message and exits

EOF
}

# parameters
while [ -n "$1" ]; do
  case $1 in
  -f | --file)
    shift
    SESSION_FILE=$1
    ;;
  -t | --table)
    shift
    SESSION_TABLE=$1
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

if [ -z "$SESSION_FILE" ]; then
  echo "Please provide a filename for the session file to upload."
  echo
  usage
  exit 1
fi

if [ -z "$SESSION_TABLE" ]; then
  echo "Please provide the name of the dynamodb table to upload to."
  echo
  usage
  exit 1
fi

# upload to dynamo db
echo "Uploading $SESSION_FILE to $SESSION_TABLE..."
aws dynamodb put-item --region eu-west-2 --table-name $SESSION_TABLE --item file://$SESSION_FILE
