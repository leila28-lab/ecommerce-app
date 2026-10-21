# Build context = repository root:
#   docker build -t ecommerce-catalog:dev -f src/ECommerce.Catalog.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from the project files only, so this layer is cached until a csproj changes.
COPY src/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj src/ECommerce.Catalog.Api/
COPY src/ECommerce.ServiceDefaults/ECommerce.ServiceDefaults.csproj src/ECommerce.ServiceDefaults/
RUN dotnet restore src/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj

COPY src/ECommerce.Catalog.Api/ src/ECommerce.Catalog.Api/
COPY src/ECommerce.ServiceDefaults/ src/ECommerce.ServiceDefaults/
RUN dotnet publish src/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj \
    -c Release --no-restore -o /app /p:UseAppHost=false

# Chiseled runtime: no shell, no package manager, non-root user ($APP_UID = 1654).
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .

# No connection string or password in the image: they are injected at startup.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ECommerce.Catalog.Api.dll"]
