FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 3000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["azure-ad-keyvault-viewer-api.csproj", "."]
RUN dotnet restore "./azure-ad-keyvault-viewer-api.csproj"
COPY . . 
WORKDIR "/src/."
RUN dotnet build "./azure-ad-keyvault-viewer-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./azure-ad-keyvault-viewer-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "azure-ad-keyvault-viewer-api.dll"]
