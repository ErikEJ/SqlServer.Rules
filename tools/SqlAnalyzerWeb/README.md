# T-SQL Analyzer demo web app

A small ASP.NET Core (.NET 10) app: a page where you paste T-SQL and get the analysis findings. Scripts are analyzed only (never executed) and not stored.

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

When running behind a reverse proxy, configure its actual trusted address or network so forwarded client IPs are used for rate limiting. Set `ForwardedHeaders__KnownProxies__0` to a proxy IP, or `ForwardedHeaders__KnownNetworks__0` to a CIDR network (for example, `10.0.0.0/8`). Add further entries with `__1`, `__2`, and so on. Do not trust arbitrary forwarded headers; configure only the proxy addresses or ranges used by your deployment.

## Deploy to Azure Container Apps

The repository includes a Dockerfile and `.github\workflows\web-demo.yml`. The workflow builds and publishes the image to GitHub Container Registry (GHCR); it does **not** deploy to Azure.

### 1. Publish the image in GitHub

1. Push changes to the web app or analyzer library to `master`, or manually run **Actions > Demo web app image > Run workflow**.
2. Confirm the workflow succeeds.
3. Open the repository owner's **Packages > tsql-analyzer-demo > Package settings** and change the package visibility to **Public**.

For this repository, the image is:

```text
ghcr.io/erikej/tsql-analyzer-demo:latest
```

For a fork, replace `erikej` with the lowercase repository owner. A public image lets Azure pull it without registry credentials. The publishing workflow uses `GITHUB_TOKEN`; no personal access token is needed. GitHub Actions must be enabled and permitted to publish packages by any applicable repository or organization policies.

### 2. Create the Azure resources

You need an active Azure subscription and permission to create resources, such as **Contributor** on the target resource group. Creating a resource group or registering resource providers may require additional subscription-level permissions.

In the Azure portal, create a **Container App** with these settings:

| Setting | Value |
| --- | --- |
| Resource group | New or existing, for example `rg-tsql-demo` |
| Container Apps environment | Create a new environment |
| Workload profile | Consumption |
| Image source | Other registry |
| Registry server | `ghcr.io` |
| Image | `erikej/tsql-analyzer-demo:latest` |
| Registry authentication | None (public image) |
| CPU / memory | Start with 0.5 vCPU / 1 GiB |
| Ingress | Enabled, external |
| Ingress transport | HTTP / Auto |
| Target port | `8080` |
| Insecure connections | Disabled |
| Minimum replicas | `0` |
| Maximum replicas | `2` |

If prompted, register the `Microsoft.App` and `Microsoft.OperationalInsights` resource providers, or ask your subscription administrator to do so.

After creation, open the app's **Application URL**. Its `/healthz` endpoint should return `"ok"`. Paste a SQL script into the page and confirm that analysis returns findings.

Consumption includes a monthly free grant, but hosting is **not guaranteed to be free**. Usage beyond the grant and logging can incur charges. Set a subscription budget alert. Scaling to zero also means cold starts.

Configure the forwarded-header environment variables described above using the **actual trusted ingress proxy address or network** for your Container Apps environment. Without this configuration, multiple visitors may share the proxy's rate limit. Do not blindly trust all forwarded headers or copy the example network without verifying it applies to your environment.

### 3. Update the deployment

Publishing another `latest` image does **not** automatically update the running Container App. For reliable releases, extend the workflow to publish an additional image tag using the Git commit SHA, then update the Container App to that specific tag. Retain `latest` if you still want it for initial setup.

### 4. Optional: automate deployment with GitHub Actions

Use **OpenID Connect (OIDC)** instead of storing an Azure password in GitHub.

In Azure:

- Create an Entra app registration and service principal, or a user-assigned managed identity.
- Add a GitHub federated credential restricted to this repository and the `master` branch. For a branch-based deployment from this repository, the subject is `repo:ErikEJ/SqlServer.Rules:ref:refs/heads/master` and the audience is `api://AzureADTokenExchange`.
- Give the deployment identity **Contributor on the web app's resource group**, not the entire subscription. Assigning this role requires an administrator with permission to create role assignments.

In GitHub:

- Add repository Actions variables named `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_RESOURCE_GROUP`, and `AZURE_CONTAINER_APP_NAME`.
- Give the deployment job `id-token: write`, retaining the existing permissions needed for checkout and image publishing.
- Add `azure/login@v2`, configured with the client, tenant, and subscription ID variables.
- After publishing the SHA-tagged image, add an Azure CLI step to update the app:

```sh
az containerapp update \
  --name "$AZURE_CONTAINER_APP_NAME" \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --image "ghcr.io/erikej/tsql-analyzer-demo:$GITHUB_SHA"
```

Map the resource group and app name Actions variables into the step's environment. Use the lowercase repository owner instead of `erikej` for a fork. If you later use a GitHub environment for deployment, update the federated credential to match that environment's subject.

For the first deployment, a public GHCR image plus Azure portal setup is enough; no Azure credentials in GitHub are required.

## Limits

Scripts are limited to 8 KB (8,192 characters), requests to 64 KB, 10 requests per minute per client IP, 2 concurrent analyses, and a 30 second timeout.
