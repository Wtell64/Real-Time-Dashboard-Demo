A .NET data monitoring project with technologies including gRPC, REST APIs, WebSockets, Avalonia UI, Docker, Kubernetes that results in an event-driven architecture.

## Technologies Demonstrated

- **gRPC Service** - High-performance RPC for system metrics
- **REST API** - Standard HTTP endpoints with Swagger documentation  
- **WebSockets** - Real-time dashboard updates via SignalR
- **Avalonia UI** - Cross-platform desktop client
- **Docker & Kubernetes** - Containerization and orchestration
- **Databases**: PostgreSQL (relational), MongoDB (documents), Redis (cache)
- **Event-Driven**: MQTT, RabbitMQ messaging
- **Background Services** - Automated metric collection every 2 minutes

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Avalonia UI   │────│   REST API      │────│   PostgreSQL    │
│   (Desktop)     │    │   WebSockets    │    │   (Metrics)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │              ┌─────────────────┐    ┌─────────────────┐
         └──────────────│   gRPC Service  │────│     Redis       │
                        │   (Streaming)   │    │   (Cache)       │
                        └─────────────────┘    └─────────────────┘
                                 │                       │
                        ┌─────────────────┐    ┌─────────────────┐
                        │  Event Service  │────│    MongoDB      │
                        │  MQTT/RabbitMQ  │    │   (Events)      │
                        └─────────────────┘    └─────────────────┘
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022
- Docker Desktop
- kubectl (for Kubernetes deployment)

### Local Development

1. **Clone and build**:
   ```bash
   git clone <repository>
   cd SystemMonitorPOC
   dotnet restore
   dotnet build
   ```

2. **Start infrastructure**:
   ```bash
   cd docker
   docker-compose up -d postgres redis mongodb rabbitmq mosquitto
   ```

3. **Run services**:
   ```bash
   # Terminal 1 - gRPC Service
   cd SystemMonitor.GrpcService
   dotnet run
   
   # Terminal 2 - Web API
   cd SystemMonitor.WebApi
   dotnet run
   
   # Terminal 3 - Event Service
   cd SystemMonitor.EventService
   dotnet run
   
   # Terminal 4 - Avalonia UI
   cd SystemMonitor.AvaloniaUI
   dotnet run
   ```

### Docker Deployment

```bash
# Build images
./build-docker.sh

# Deploy with Docker Compose
./deploy-local.sh
```

### Kubernetes Deployment

```bash
# Deploy to K8s
./deploy-k8s.sh

# Check status
kubectl get all -n system-monitor
```

## 🎯 Features

- **Real-time Metrics**: CPU, Memory, Disk usage updated every 2 minutes
- **Multiple Protocols**: REST, gRPC, WebSocket, MQTT all working together
- **Event-Driven**: Automatic alerts for high resource usage
- **Modern UI**: Beautiful Avalonia dashboard with live charts
- **Cloud-Ready**: Full Docker and Kubernetes support
- **High Performance**: Redis caching, connection pooling, async operations

## 📊 Monitoring

- **Web API**: http://localhost:5000/swagger
- **gRPC**: http://localhost:5001 (HTTP/2)
- **RabbitMQ Management**: http://localhost:15672 (admin/admin123)
- **Desktop Dashboard**: Launch Avalonia app

