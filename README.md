# uw-homework

## Prerequisites

Docker is recommended

`dotnet tool install --global dotnet-ef`
```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<password>" `
   -p 1433:1433 --name sql1 --hostname sql1 `
   -d `
   mcr.microsoft.com/mssql/server:2025-latest
```

For the catalog and product projects, each

`dotnet user-secrets init`
`dotnet user-secrets set "ConnectionStrings:ProductDb" "Server=.;Database=ProductDb;Trusted_Connection=false;MultipleActiveResultSets=true;User ID=sa;Password=<password>;TrustServerCertificate=true;`

