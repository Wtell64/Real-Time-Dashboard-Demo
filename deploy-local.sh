#!/bin/bash

echo "Deploying SystemMonitor locally..."

# Start infrastructure
echo "Starting infrastructure services..."
cd docker
docker-compose up -d postgres redis mongodb rabbitmq mosquitto

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 30

# Start application services
echo "Starting application services..."
docker-compose up -d grpc-service webapi event-service

echo "Local deployment completed!"
echo "Services available at:"
echo "  - Web API: http://localhost:5000"
echo "  - Swagger UI: http://localhost:5000/swagger"
echo "  - gRPC Service: http://localhost:5001"
echo "  - RabbitMQ Management: http://localhost:15672 (admin/admin123)"