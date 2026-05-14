FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SaasCommerce.slnx Directory.Build.props global.json ./
COPY backend/src/SaasCommerce.Domain/SaasCommerce.Domain.csproj backend/src/SaasCommerce.Domain/
COPY backend/src/SaasCommerce.Application/SaasCommerce.Application.csproj backend/src/SaasCommerce.Application/
COPY backend/src/SaasCommerce.Contracts/SaasCommerce.Contracts.csproj backend/src/SaasCommerce.Contracts/
COPY backend/src/SaasCommerce.Infrastructure/SaasCommerce.Infrastructure.csproj backend/src/SaasCommerce.Infrastructure/
COPY backend/src/Worker/SaasCommerce.Worker.csproj backend/src/Worker/
RUN dotnet restore backend/src/Worker/SaasCommerce.Worker.csproj

COPY . .
RUN dotnet publish backend/src/Worker/SaasCommerce.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SaasCommerce.Worker.dll"]
