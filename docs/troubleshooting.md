# Troubleshooting

## Process not found

The target PID, process name, or service name did not resolve to a live process. Re-run `dss list` and verify the target is still running.

## Multiple processes matched

`--name` matched more than one process. Use `--pid` or refine the process name.

## Not a .NET process

The target is not currently published through the local .NET diagnostics transport. Confirm it is a managed .NET process and not already exiting.

## Permission denied

The current user cannot attach to the target process. Try running `dss` with appropriate privileges or under the same account as the target.

## No counters received

The process was attachable but did not emit the expected `System.Runtime` counters during the capture window. Increase `--duration` or retry while the process is active.

## Target exited during capture

The process ended while `dss` was collecting data. If partial process metrics were available, `dss` reports a partial snapshot.

## Linux service targeting

`dss snap --service ...` is not implemented on Linux in V1. Resolve the target with `dss list` and then use `--pid` or `--name`.
