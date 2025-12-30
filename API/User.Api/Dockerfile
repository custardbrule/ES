FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5189
EXPOSE 7155

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Install Node.js for Tailwind CSS
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

WORKDIR /src

# Copy only project files first for better layer caching
# This allows Docker to cache the restore layer if only source code changes
COPY ["API/User.Api/User.Api.csproj", "API/User.Api/"]
COPY ["Infrastructure/Infras.User.Services/Infras.User.Services.csproj", "Infrastructure/Infras.User.Services/"]
COPY ["Infrastructure/Data/Data.csproj", "Infrastructure/Data/"]
COPY ["Domain/Domain.User/Domain.User.csproj", "Domain/Domain.User/"]
COPY ["Domain/Seed/Seed.csproj", "Domain/Seed/"]
COPY ["Shared/CQRS/CQRS.csproj", "Shared/CQRS/"]
COPY ["Shared/Utilities/Utilities.csproj", "Shared/Utilities/"]
COPY ["Shared/RequestValidator/RequestValidator.csproj", "Shared/RequestValidator/"]
COPY ["Shared/DataStore/DataStore.csproj", "Shared/DataStore/"]

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore "API/User.Api/User.Api.csproj"

# Copy all source code files
COPY . .

# Install npm dependencies and build Tailwind CSS
WORKDIR "/src/API/User.Api"
RUN if [ -f "package.json" ]; then npm install; fi

# Build
RUN dotnet build "User.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "User.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.Api.dll"]
