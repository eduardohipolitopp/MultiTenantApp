FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MultiTenantApp.Web/MultiTenantApp.Web.csproj", "src/MultiTenantApp.Web/"]
COPY ["src/MultiTenantApp.Application/MultiTenantApp.Application.csproj", "src/MultiTenantApp.Application/"]
COPY ["src/MultiTenantApp.Domain/MultiTenantApp.Domain.csproj", "src/MultiTenantApp.Domain/"]
RUN dotnet restore "src/MultiTenantApp.Web/MultiTenantApp.Web.csproj"
COPY . .
WORKDIR "/src/src/MultiTenantApp.Web"
RUN dotnet build "MultiTenantApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiTenantApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiTenantApp.Web.dll"]
