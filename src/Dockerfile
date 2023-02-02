FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CyclingStats.Workers/CyclingStats.Workers.csproj", "CyclingStats.Workers/"]
RUN dotnet restore "CyclingStats.Workers/CyclingStats.Workers.csproj"
COPY . .
WORKDIR "/src/CyclingStats.Workers"
RUN dotnet build "CyclingStats.Workers.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CyclingStats.Workers.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CyclingStats.Workers.dll"]