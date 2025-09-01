#!/bin/bash

echo "Building SystemMonitor Docker images..."

# Build gRPC Service
echo "Building gRPC Service..."
docker build -t systemmonitor/grpc-service:latest -f SystemMonitor.GrpcService/Dockerfile .

# Build Web API
echo "Building Web API..."
docker build -t systemmonitor/webapi:latest -f SystemMonitor.WebApi/Dockerfile .

# Build Event Service
echo "Building Event Service..."
docker build -t systemmonitor/event-service:latest -f SystemMonitor.EventService/Dockerfile .

echo "All Docker images built successfully!"