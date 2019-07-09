FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-stretch AS build
WORKDIR /src
COPY ["DataHub.Api/DataHub.Api.csproj", "DataHub.Api/"]
RUN dotnet restore "DataHub.Api/DataHub.Api.csproj"
COPY . .
WORKDIR "/src/DataHub.Api"
RUN dotnet build "DataHub.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DataHub.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataHub.Api.dll"]