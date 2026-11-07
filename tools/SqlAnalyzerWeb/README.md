# T-SQL Analyzer demo web app

A small ASP.NET Core (.NET 10) app: a page where you paste T-SQL and get the analysis findings. Scripts are analyzed only (never executed) and not stored.

GitHub Pages only serves static files, so the app is hosted as a container on a free tier, and Pages can optionally link to it.

## Run locally

```pwsh
dotnet run --project tools/SqlAnalyzerWeb
```

## Container

```pwsh
docker build -f tools/SqlAnalyzerWeb/Dockerfile -t tsql-analyzer-demo .
docker run --rm -p 8080:8080 tsql-analyzer-demo
```

## Native runtime requirements

- .NET 10 ASP.NET Core runtime (the `mcr.microsoft.com/dotnet/aspnet:10.0` image) - DacFx is managed code and runs on Linux.
- **ICU (`libicu`)** is required. DacFx creates cultures by LCID, which fails with `CultureNotFoundException` under `InvariantGlobalization=true` or on images without ICU (Alpine, chiseled/distroless). Use the Debian-based image, or install `libicu` and keep `InvariantGlobalization` off.
- No SQL Server connection or other native libraries are needed for script analysis.
- Memory: allow roughly 512 MB; the first request is slow (model/MEF initialization).

## Hosting (free tiers)

The `web-demo.yml` workflow pushes `ghcr.io/<owner>/tsql-analyzer-demo:latest`. Any of these can run it (expect cold starts):

- **Render** (free web service): create a Web Service from this repo, runtime Docker, Dockerfile path `tools/SqlAnalyzerWeb/Dockerfile`, root directory `.`. Set the health check path to `/healthz`.
- **Azure Container Apps** (consumption, scale to zero, monthly free grant): create an app from the GHCR image, target port `8080`, ingress external, min replicas `0`.
- **Fly.io**: `fly launch --dockerfile tools/SqlAnalyzerWeb/Dockerfile` with internal port `8080`.

## Limits

Scripts are limited to 8 KB (8,192 characters), requests to 64 KB, 10 requests per minute per client IP, 2 concurrent analyses, and a 30 second timeout.
