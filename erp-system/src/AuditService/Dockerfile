# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/AuditService/AuditService.API/AuditService.API.csproj", "src/AuditService/AuditService.API/"]
COPY ["src/AuditService/AuditService.Application/AuditService.Application.csproj", "src/AuditService/AuditService.Application/"]
COPY ["src/AuditService/AuditService.Domain/AuditService.Domain.csproj", "src/AuditService/AuditService.Domain/"]
COPY ["src/AuditService/AuditService.Infrastructure/AuditService.Infrastructure.csproj", "src/AuditService/AuditService.Infrastructure/"]

RUN dotnet restore "src/AuditService/AuditService.API/AuditService.API.csproj"

# Copy the source code
COPY . .
WORKDIR "/src/src/AuditService/AuditService.API"

# Build the application
RUN dotnet build "AuditService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuditService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create the final image
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r auditservice && useradd -r -g auditservice auditservice

# Copy the published application
COPY --from=publish /app/publish .

# Set ownership of the app directory
RUN chown -R auditservice:auditservice /app

# Switch to non-root user
USER auditservice

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "AuditService.API.dll"]
