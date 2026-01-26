# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MultiTenantApp.Hangfire/MultiTenantApp.Hangfire.csproj", "src/MultiTenantApp.Hangfire/"]
COPY ["src/MultiTenantApp.Domain/MultiTenantApp.Domain.csproj", "src/MultiTenantApp.Domain/"]
COPY ["src/MultiTenantApp.Application/MultiTenantApp.Application.csproj", "src/MultiTenantApp.Application/"]
COPY ["src/MultiTenantApp.Infrastructure/MultiTenantApp.Infrastructure.csproj", "src/MultiTenantApp.Infrastructure/"]

RUN dotnet restore "src/MultiTenantApp.Hangfire/MultiTenantApp.Hangfire.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/MultiTenantApp.Hangfire"
RUN dotnet build "MultiTenantApp.Hangfire.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MultiTenantApp.Hangfire.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "MultiTenantApp.Hangfire.dll"]