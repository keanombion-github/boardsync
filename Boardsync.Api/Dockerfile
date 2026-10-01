FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY Boardsync.Api/Boardsync.Api.csproj Boardsync.Api/
RUN dotnet restore Boardsync.Api/Boardsync.Api.csproj

COPY Boardsync.Api/ Boardsync.Api/
RUN dotnet publish Boardsync.Api/Boardsync.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
USER app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Boardsync.Api.dll"]
