FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SaasCommerce.slnx Directory.Build.props global.json ./
COPY backend/src/SharedKernel/SaasCommerce.SharedKernel.csproj backend/src/SharedKernel/
COPY backend/src/BuildingBlocks/SaasCommerce.BuildingBlocks.csproj backend/src/BuildingBlocks/
COPY backend/src/Modules/SaasCommerce.Modules.csproj backend/src/Modules/
COPY backend/src/Worker/SaasCommerce.Worker.csproj backend/src/Worker/
RUN dotnet restore backend/src/Worker/SaasCommerce.Worker.csproj

COPY backend/src/SharedKernel/ backend/src/SharedKernel/
COPY backend/src/BuildingBlocks/ backend/src/BuildingBlocks/
COPY backend/src/Modules/ backend/src/Modules/
COPY backend/src/Worker/ backend/src/Worker/
RUN dotnet publish backend/src/Worker/SaasCommerce.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
  && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
  && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SaasCommerce.Worker.dll"]
