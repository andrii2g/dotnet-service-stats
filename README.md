# dotnet-service-stats

`dss` is a console application for local diagnostics triage. It attaches to a running local .NET process, collects a short `System.Runtime` EventPipe counter sample, and prints either a readable summary or stable JSON.

`dss` attaches using local .NET diagnostics infrastructure and collects a short EventPipe counter sample. It does not modify the target application and does not require code changes in the target application.

## What It Does

- Lists likely attachable local .NET processes.
- Captures a short runtime and process snapshot for one target process.
- Emits either console output or JSON for automation.
- Supports `list`, `snap --pid`, and `snap --name` on Windows and Linux.
- Reports core runtime metrics including GC heap, LOH size, GC pause percentage, heap fragmentation, active timer count, and JIT activity.

## What It Does Not Do

- Live dashboards or watch mode.
- Dumps, traces, or remote attach.
- ASP.NET Core, Kestrel, EF Core, or OpenTelemetry-specific analysis in V1.

## Build

Project naming:

- Repository: `dotnet-service-stats`
- Project path: `src/ServiceStats`
- Test project path: `tests/ServiceStats.Tests`
- Built executable name: `dss`

Build from source:

```bash
dotnet build ServiceStats.slnx -c Release
```

## Usage

Run directly from source:

```bash
dotnet run --project src/ServiceStats/ServiceStats.csproj -- list
dotnet run --project src/ServiceStats/ServiceStats.csproj -- snap --pid 1234
```

Run the built executable:

Windows:

```powershell
.\src\ServiceStats\bin\Release\net10.0\dss.exe list
.\src\ServiceStats\bin\Release\net10.0\dss.exe snap --pid 1234
```

Linux:

```bash
./src/ServiceStats/bin/Release/net10.0/dss list
./src/ServiceStats/bin/Release/net10.0/dss snap --pid 1234
```

Supported commands:

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
