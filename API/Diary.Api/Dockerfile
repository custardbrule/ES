FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5075
EXPOSE 7119

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project files first for better layer caching
# This allows Docker to cache the restore layer if only source code changes
COPY ["API/Diary.Api/Diary.Api.csproj", "API/Diary.Api/"]
COPY ["Infrastructure/Infras.Diary.Services/Infras.Diary.Services.csproj", "Infrastructure/Infras.Diary.Services/"]
COPY ["Infrastructure/Data/Data.csproj", "Infrastructure/Data/"]
COPY ["Domain/Domain.Diary/Domain.Diary.csproj", "Domain/Domain.Diary/"]
COPY ["Shared/CQRS/CQRS.csproj", "Shared/CQRS/"]
COPY ["Shared/Utilities/Utilities.csproj", "Shared/Utilities/"]
COPY ["Shared/RequestValidator/RequestValidator.csproj", "Shared/RequestValidator/"]
COPY ["Shared/DataStore/DataStore.csproj", "Shared/DataStore/"]

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore "API/Diary.Api/Diary.Api.csproj"

# Copy all source code files
COPY . .

# Build
WORKDIR "/src/API/Diary.Api"
RUN dotnet build "Diary.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Diary.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Diary.Api.dll"]
