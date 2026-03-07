# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY GPMS_BE.sln ./
COPY GMPS.API/GMPS.API.csproj GMPS.API/
COPY GPMS.APPLICATION/GPMS.APPLICATION.csproj GPMS.APPLICATION/
COPY GPMS.DOMAIN/GPMS.DOMAIN.csproj GPMS.DOMAIN/
COPY GPMS.INFRASTRUCTURE/GPMS.INFRASTRUCTURE.csproj GPMS.INFRASTRUCTURE/

RUN dotnet restore GMPS.API/GMPS.API.csproj

# Copy remaining source code and publish
COPY . .
RUN dotnet publish GMPS.API/GMPS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GMPS.API.dll"]