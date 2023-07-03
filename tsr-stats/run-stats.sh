#!/bin/bash

SUMMARY_TABLE_NAME=tsr-importer-app-SummaryTable-6TXVBV7ENKKK
SESSION_TABLE_NAME=tsr-data-platform
S3_BUCKET_NAME=collective-simulation-tsr-data-uploads

dotnet run --project TsrStats/TsrStats.csproj -- $S3_BUCKET_NAME $SESSION_TABLE_NAME $SUMMARY_TABLE_NAME
# dotnet run --project TsrStats/TsrStats.csproj -- "$@"

