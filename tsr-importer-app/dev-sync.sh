#!/bin/bash

echo "Syncing with stack: $1"
sam sync --stack-name $1 --watch --region eu-west-2
