FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NoSQL.WebApi.csproj", "."]
RUN dotnet restore "./NoSQL.WebApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./NoSQL.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NoSQL.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure XML documentation file is present for Swagger
COPY --from=publish /app/publish/NoSQL.WebApi.xml .

ENTRYPOINT ["dotnet", "NoSQL.WebApi.dll"]