# Use the official .NET 10 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 10 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/HRService/HRService.API/HRService.API.csproj", "src/HRService/HRService.API/"]
COPY ["src/HRService/HRService.Application/HRService.Application.csproj", "src/HRService/HRService.Application/"]
COPY ["src/HRService/HRService.Domain/HRService.Domain.csproj", "src/HRService/HRService.Domain/"]
COPY ["src/HRService/HRService.Infrastructure/HRService.Infrastructure.csproj", "src/HRService/HRService.Infrastructure/"]

RUN dotnet restore "src/HRService/HRService.API/HRService.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/HRService/HRService.API"
RUN dotnet build "HRService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HRService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - create the runtime image
FROM base AS final
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HRService.API.dll"]
