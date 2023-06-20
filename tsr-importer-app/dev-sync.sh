#!/bin/bash

echo "Syncing the application stack on AWS..."
sam sync --stack-name tsr-importer-app --watch --region eu-west-2
