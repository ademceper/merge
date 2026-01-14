# ✅ BOLUM 6.0: Dockerfile (ZORUNLU)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Merge.API/Merge.API.csproj", "Merge.API/"]
COPY ["Merge.Application/Merge.Application.csproj", "Merge.Application/"]
COPY ["Merge.Domain/Merge.Domain.csproj", "Merge.Domain/"]
COPY ["Merge.Infrastructure/Merge.Infrastructure.csproj", "Merge.Infrastructure/"]

RUN dotnet restore "Merge.API/Merge.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Merge.API"
RUN dotnet build "Merge.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Merge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ✅ SECURITY: Non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Merge.API.dll"]

