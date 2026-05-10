# ─── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first — Docker caches this layer until .csproj files change
COPY src/LMS.Domain/LMS.Domain.csproj src/LMS.Domain/
COPY src/LMS.Application/LMS.Application.csproj src/LMS.Application/
COPY src/LMS.Infrastructure/LMS.Infrastructure.csproj src/LMS.Infrastructure/
COPY src/LMS.API/LMS.API.csproj src/LMS.API/

# Restore dependencies
RUN dotnet restore src/LMS.API/LMS.API.csproj

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish src/LMS.API/LMS.API.csproj -c Release -o /app/publish

# ─── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Create uploads directory (for local file storage in MVP)
RUN mkdir -p /app/uploads

EXPOSE 5001

ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "LMS.API.dll"]
