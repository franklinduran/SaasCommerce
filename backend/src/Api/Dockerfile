FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SaasCommerce.slnx Directory.Build.props global.json ./
COPY backend/src/SaasCommerce.Domain/SaasCommerce.Domain.csproj backend/src/SaasCommerce.Domain/
COPY backend/src/SaasCommerce.Application/SaasCommerce.Application.csproj backend/src/SaasCommerce.Application/
COPY backend/src/SaasCommerce.Contracts/SaasCommerce.Contracts.csproj backend/src/SaasCommerce.Contracts/
COPY backend/src/SaasCommerce.Infrastructure/SaasCommerce.Infrastructure.csproj backend/src/SaasCommerce.Infrastructure/
COPY backend/src/Api/SaasCommerce.Api.csproj backend/src/Api/
RUN dotnet restore backend/src/Api/SaasCommerce.Api.csproj

COPY . .
RUN dotnet publish backend/src/Api/SaasCommerce.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SaasCommerce.Api.dll"]
