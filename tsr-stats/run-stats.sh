#!/bin/bash

SUMMARY_TABLE_NAME=tsr-importer-app-SummaryTable-6TXVBV7ENKKK
SESSION_TABLE_NAME=tsr-data-platform

dotnet run --project TsrStats/TsrStats.csproj -- $SESSION_TABLE_NAME $SUMMARY_TABLE_NAME
# dotnet run --project TsrStats/TsrStats.csproj -- "$@"

