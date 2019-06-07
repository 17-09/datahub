FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["DataHub.Server/DataHub.Server.csproj", "DataHub.Server/"]
RUN dotnet restore "DataHub.Server/DataHub.Server.csproj"
COPY . .
WORKDIR "/src/DataHub.Server"
RUN dotnet build "DataHub.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DataHub.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataHub.Server.dll"]