FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitQueue.WebApi.csproj", "."]
RUN dotnet restore "./RabbitQueue.WebApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./RabbitQueue.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RabbitQueue.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure XML documentation file is present for Swagger
COPY --from=publish /app/publish/RabbitQueue.WebApi.xml .

ENTRYPOINT ["dotnet", "RabbitQueue.WebApi.dll"]