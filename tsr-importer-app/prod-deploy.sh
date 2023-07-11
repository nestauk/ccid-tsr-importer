#!/bin/bash

PROD_STACK_NAME="tsr-importer-app"

echo "Building application..."
sam build --region eu-west-2

echo "Deploying the application to stack: $PROD_STACK_NAME..."
sam deploy --region eu-west-2 --stack-name $PROD_STACK_NAME

# sam sync --stack-name $PROD_STACK_NAME --watch --region eu-west-2
