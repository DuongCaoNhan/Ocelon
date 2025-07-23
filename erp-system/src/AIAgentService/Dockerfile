# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AIAgentService.API/AIAgentService.API.csproj", "AIAgentService.API/"]
COPY ["AIAgentService.Application/AIAgentService.Application.csproj", "AIAgentService.Application/"]
COPY ["AIAgentService.Domain/AIAgentService.Domain.csproj", "AIAgentService.Domain/"]
COPY ["AIAgentService.Infrastructure/AIAgentService.Infrastructure.csproj", "AIAgentService.Infrastructure/"]

RUN dotnet restore "AIAgentService.API/AIAgentService.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/AIAgentService.API"
RUN dotnet build "AIAgentService.API.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "AIAgentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the base image and copy the published app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' --shell /bin/bash --uid 1001 appuser
RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "AIAgentService.API.dll"]
