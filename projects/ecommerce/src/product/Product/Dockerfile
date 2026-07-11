## Multi-stage Dockerfile for the Product API
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and build the Product project
COPY . .

# Restore and publish
WORKDIR /src/Product

RUN ls -l .

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Listen on port 80 by default. You can override ASPNETCORE_URLS to enable HTTPS or another port.
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

# Entry point
ENTRYPOINT ["dotnet", "Product.dll"]
