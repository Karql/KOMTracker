FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["KomTracker/KomTracker.WEB/KomTracker.WEB.csproj", "KomTracker/KomTracker.WEB/"]
COPY ["KomTracker/KomTracker.API.Shared/KomTracker.API.Shared.csproj", "KomTracker/KomTracker.API.Shared/"]
COPY ["KomTracker/KomTracker.Infrastructure.Shared/KomTracker.Infrastructure.Shared.csproj", "KomTracker/KomTracker.Infrastructure.Shared/"]
COPY ["KomTracker/KomTracker.Application.Shared/KomTracker.Application.Shared.csproj", "KomTracker/KomTracker.Application.Shared/"]
RUN dotnet restore "KomTracker/KomTracker.WEB/KomTracker.WEB.csproj"
COPY . .
WORKDIR "/src/KomTracker/KomTracker.WEB"
RUN dotnet build "KomTracker.WEB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KomTracker.WEB.csproj" -c Release -o /app/publish

FROM nginx:1.21-alpine AS final
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish/wwwroot .
COPY KomTracker/KomTracker.WEB/Docker/nginx/nginx.conf /etc/nginx/nginx.conf