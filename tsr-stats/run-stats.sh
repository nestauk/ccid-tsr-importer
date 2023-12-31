#!/bin/bash

SUMMARY_TABLE_NAME=tsr-importer-app-SummaryTable-6TXVBV7ENKKK
SESSION_TABLE_NAME=tsr-importer-app-SessionTable-O89MWVA1W5BQ
S3_BUCKET_NAME=collective-simulation-tsr-data-uploads
DATA_ENDPOINT=https://wx2igcevv7.execute-api.eu-west-2.amazonaws.com/Prod/summary

dotnet run --project TsrStats/TsrStats.csproj -- $S3_BUCKET_NAME $SESSION_TABLE_NAME $SUMMARY_TABLE_NAME $DATA_ENDPOINT
# dotnet run --project TsrStats/TsrStats.csproj -- "$@"
