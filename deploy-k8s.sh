#!/bin/bash

echo "Deploying SystemMonitor to Kubernetes..."

# Apply Kubernetes manifests
kubectl apply -f k8s/namespace.yaml

echo "Deploying infrastructure..."
kubectl apply -f k8s/ -n system-monitor

echo "Waiting for deployments to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment --all -n system-monitor

echo "Kubernetes deployment completed!"
echo "Check status with: kubectl get all -n system-monitor"
