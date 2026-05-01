# dotnet-service-stats

`dss` is a small .NET global tool for local diagnostics triage. It attaches to a running local .NET process, collects a short `System.Runtime` EventPipe counter sample, and prints either a readable summary or stable JSON.

`dss` attaches using local .NET diagnostics infrastructure and collects a short EventPipe counter sample. It does not modify the target application and does not require code changes in the target application.

## What It Does

- Lists likely attachable local .NET processes.
- Captures a short runtime and process snapshot for one target process.
- Emits either console output or JSON for automation.
- Supports `list`, `snap --pid`, and `snap --name` on Windows and Linux.

## What It Does Not Do

- Live dashboards or watch mode.
- Dumps, traces, or remote attach.
- ASP.NET Core, Kestrel, EF Core, or OpenTelemetry-specific analysis in V1.

## Install

```bash
dotnet tool install --global A2G.ServiceStats
```

For local development from this repo:

```bash
dotnet pack src/A2G.ServiceStats/A2G.ServiceStats.csproj -c Release
dotnet tool install --global --add-source ./src/A2G.ServiceStats/nupkg A2G.ServiceStats
```

## Usage

```bash
dss list
dss list --json
dss snap --pid 1234
dss snap --name Orders.Api
dss snap --pid 1234 --json
```

## Permissions

`dss` only works against local processes that publish .NET diagnostics endpoints and that your current user can access. Elevated processes or processes running as another account may require elevated privileges.

On Linux, `--service` is intentionally not supported in V1. Use `--pid` or `--name` instead.

## Docs

- [limitations](docs/limitations.md)
- [troubleshooting](docs/troubleshooting.md)
- [metrics roadmap](docs/metrics-roadmap.md)
- [implementation plan](docs/PLAN.md)
