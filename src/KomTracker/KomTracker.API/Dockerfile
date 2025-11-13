#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["KomTracker/KomTracker.API/KomTracker.API.csproj", "KomTracker/KomTracker.API/"]
COPY ["KomTracker/KomTracker.API.Shared/KomTracker.API.Shared.csproj", "KomTracker/KomTracker.API.Shared/"]
COPY ["KomTracker/KomTracker.Infrastructure/KomTracker.Infrastructure.csproj", "KomTracker/KomTracker.Infrastructure/"]
COPY ["KomTracker/KomTracker.Infrastructure.Shared/KomTracker.Infrastructure.Shared.csproj", "KomTracker/KomTracker.Infrastructure.Shared/"]
COPY ["KomTracker/KomTracker.Application/KomTracker.Application.csproj", "KomTracker/KomTracker.Application/"]
COPY ["KomTracker/KomTracker.Application.Shared/KomTracker.Application.Shared.csproj", "KomTracker/KomTracker.Application.Shared/"]
COPY ["KomTracker/KomTracker.Domain/KomTracker.Domain.csproj", "KomTracker/KomTracker.Domain/"]
COPY ["Utils/Utils/Utils.csproj", "Utils/Utils/"]
COPY ["Strava/Strava.API.Client/Strava.API.Client.csproj", "Strava/Strava.API.Client/"]
RUN dotnet restore "KomTracker/KomTracker.API/KomTracker.API.csproj"
COPY . .
WORKDIR "/src/KomTracker/KomTracker.API"
RUN dotnet build "KomTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KomTracker.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KomTracker.API.dll"]