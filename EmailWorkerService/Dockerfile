FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EmailWorkerService/EmailWorkerService.csproj", "EmailWorkerService/"]
RUN dotnet restore "EmailWorkerService/EmailWorkerService.csproj"

COPY EmailWorkerService/. ./EmailWorkerService/
WORKDIR "/src/EmailWorkerService"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EmailWorkerService.dll"]