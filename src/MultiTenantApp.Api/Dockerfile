FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MultiTenantApp.Api/MultiTenantApp.Api.csproj", "src/MultiTenantApp.Api/"]
COPY ["src/MultiTenantApp.Application/MultiTenantApp.Application.csproj", "src/MultiTenantApp.Application/"]
COPY ["src/MultiTenantApp.Domain/MultiTenantApp.Domain.csproj", "src/MultiTenantApp.Domain/"]
COPY ["src/MultiTenantApp.Infrastructure/MultiTenantApp.Infrastructure.csproj", "src/MultiTenantApp.Infrastructure/"]
RUN dotnet restore "src/MultiTenantApp.Api/MultiTenantApp.Api.csproj"
COPY . .
WORKDIR "/src/src/MultiTenantApp.Api"
RUN dotnet build "MultiTenantApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiTenantApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiTenantApp.Api.dll"]
