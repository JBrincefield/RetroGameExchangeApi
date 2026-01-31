FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RetroGameExchangeApi/RetroGameExchangeApi.csproj", "RetroGameExchangeApi/"]
RUN dotnet restore "RetroGameExchangeApi/RetroGameExchangeApi.csproj"

COPY RetroGameExchangeApi/. ./RetroGameExchangeApi/
WORKDIR "/src/RetroGameExchangeApi"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RetroGameExchangeApi.dll"]