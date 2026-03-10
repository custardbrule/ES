FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["API/Extract.Worker/Extract.Worker.csproj", "API/Extract.Worker/"]
COPY ["Infrastructure/Infras.Extract.Services/Infras.Extract.Services.csproj", "Infrastructure/Infras.Extract.Services/"]
COPY ["Infrastructure/Data/Data.csproj", "Infrastructure/Data/"]
COPY ["Domain/Domain.Extract/Domain.Extract.csproj", "Domain/Domain.Extract/"]
COPY ["Domain/Domain.Diary/Domain.Diary.csproj", "Domain/Domain.Diary/"]
COPY ["Domain/Seed/Seed.csproj", "Domain/Seed/"]

RUN dotnet restore "API/Extract.Worker/Extract.Worker.csproj"

COPY . .

WORKDIR "/src/API/Extract.Worker"
RUN dotnet build "Extract.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Extract.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Extract.Worker.dll"]
