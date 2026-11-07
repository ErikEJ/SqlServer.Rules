# Build from the repository root:
#   docker build -f tools/SqlAnalyzerWeb/Dockerfile -t tsql-analyzer-demo .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props nuget.config ./
COPY tools/ErikEJ.DacFX.TSQLAnalyzer tools/ErikEJ.DacFX.TSQLAnalyzer
COPY tools/SqlAnalyzerWeb tools/SqlAnalyzerWeb
RUN dotnet publish tools/SqlAnalyzerWeb/SqlAnalyzerWeb.csproj -c Release -o /app /p:UseAppHost=false

# Debian-based image: DacFx needs ICU (globalization-invariant mode and
# Alpine/chiseled images without libicu fail with CultureNotFoundException).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "SqlAnalyzerWeb.dll"]
