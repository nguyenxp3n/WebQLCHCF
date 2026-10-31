# 1. Base runtime image (.NET 10 Stable)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WebQLCHCF.csproj", "./"]
RUN dotnet restore "WebQLCHCF.csproj"
COPY . .
RUN dotnet build "WebQLCHCF.csproj" -c Release -o /app/build

# 3. Publish stage
FROM build AS publish
RUN dotnet publish "WebQLCHCF.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "WebQLCHCF.dll"]
