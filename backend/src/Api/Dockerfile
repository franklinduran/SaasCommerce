FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SaasCommerce.slnx Directory.Build.props global.json ./
COPY backend/src/SharedKernel/SaasCommerce.SharedKernel.csproj backend/src/SharedKernel/
COPY backend/src/BuildingBlocks/SaasCommerce.BuildingBlocks.csproj backend/src/BuildingBlocks/
COPY backend/src/Modules/SaasCommerce.Modules.csproj backend/src/Modules/
COPY backend/src/Api/SaasCommerce.Api.csproj backend/src/Api/
RUN dotnet restore backend/src/Api/SaasCommerce.Api.csproj

COPY backend/src/SharedKernel/ backend/src/SharedKernel/
COPY backend/src/BuildingBlocks/ backend/src/BuildingBlocks/
COPY backend/src/Modules/ backend/src/Modules/
COPY backend/src/Api/ backend/src/Api/
RUN dotnet publish backend/src/Api/SaasCommerce.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
  && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
  && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SaasCommerce.Api.dll"]
