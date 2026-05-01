# Limitations

- `dss` only supports local machine diagnostics.
- The target process must expose compatible .NET diagnostics IPC endpoints.
- Some process metrics are platform-specific.
- Runtime counter availability varies by runtime version.
- Windows service name resolution is Windows-only in V1.
- Linux support in V1 is limited to `list`, `snap --pid`, and `snap --name`.
- The JSON schema is versioned and may evolve through `schemaVersion`.
