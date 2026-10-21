# Build context = repository root:
#   docker build -t ecommerce-web:dev -f src/ECommerce.Web/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from the project files only, so this layer is cached until a csproj changes.
COPY src/ECommerce.Web/ECommerce.Web.csproj src/ECommerce.Web/
COPY src/ECommerce.ServiceDefaults/ECommerce.ServiceDefaults.csproj src/ECommerce.ServiceDefaults/
RUN dotnet restore src/ECommerce.Web/ECommerce.Web.csproj

COPY src/ECommerce.Web/ src/ECommerce.Web/
COPY src/ECommerce.ServiceDefaults/ src/ECommerce.ServiceDefaults/
RUN dotnet publish src/ECommerce.Web/ECommerce.Web.csproj \
    -c Release --no-restore -o /app /p:UseAppHost=false

# Chiseled runtime: no shell, no package manager, non-root user ($APP_UID = 1654).
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .

# No connection string or password in the image: they are injected at startup.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ECommerce.Web.dll"]
