FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SystemMonitor.GrpcService/SystemMonitor.GrpcService.csproj", "SystemMonitor.GrpcService/"]
COPY ["SystemMonitor.Shared/SystemMonitor.Shared.csproj", "SystemMonitor.Shared/"]
COPY ["SystemMonitor.DataAccess/SystemMonitor.DataAccess.csproj", "SystemMonitor.DataAccess/"]

RUN dotnet restore "SystemMonitor.GrpcService/SystemMonitor.GrpcService.csproj"
COPY . .
WORKDIR "/src/SystemMonitor.GrpcService"
RUN dotnet build "SystemMonitor.GrpcService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SystemMonitor.GrpcService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SystemMonitor.GrpcService.dll"]