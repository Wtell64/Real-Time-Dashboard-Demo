FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SystemMonitor.EventService/SystemMonitor.EventService.csproj", "SystemMonitor.EventService/"]
COPY ["SystemMonitor.Shared/SystemMonitor.Shared.csproj", "SystemMonitor.Shared/"]
COPY ["SystemMonitor.DataAccess/SystemMonitor.DataAccess.csproj", "SystemMonitor.DataAccess/"]

RUN dotnet restore "SystemMonitor.EventService/SystemMonitor.EventService.csproj"
COPY . .
WORKDIR "/src/SystemMonitor.EventService"
RUN dotnet build "SystemMonitor.EventService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SystemMonitor.EventService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SystemMonitor.EventService.dll"]