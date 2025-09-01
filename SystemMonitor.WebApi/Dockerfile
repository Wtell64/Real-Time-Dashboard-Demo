FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SystemMonitor.WebApi/SystemMonitor.WebApi.csproj", "SystemMonitor.WebApi/"]
COPY ["SystemMonitor.Shared/SystemMonitor.Shared.csproj", "SystemMonitor.Shared/"]
COPY ["SystemMonitor.DataAccess/SystemMonitor.DataAccess.csproj", "SystemMonitor.DataAccess/"]

RUN dotnet restore "SystemMonitor.WebApi/SystemMonitor.WebApi.csproj"
COPY . .
WORKDIR "/src/SystemMonitor.WebApi"
RUN dotnet build "SystemMonitor.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SystemMonitor.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SystemMonitor.WebApi.dll"]