using A2G.ServiceStats.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace A2G.ServiceStats.Discovery;

internal sealed class WindowsServiceResolver
{
    public Task<int> ResolveProcessIdAsync(string serviceName, CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new DiagnosticsException(ExitCodes.PlatformNotSupported, "Service resolution is only implemented for Windows services in V1. Use --pid instead.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var manager = OpenSCManager(IntPtr.Zero, IntPtr.Zero, ServiceManagerAccess.SC_MANAGER_CONNECT);
        if (manager.IsInvalid)
        {
            throw new DiagnosticsException(ExitCodes.CollectionFailed, $"Failed to open the Windows service control manager. Win32 error: {Marshal.GetLastWin32Error()}.");
        }

        using var service = OpenService(manager, serviceName, ServiceAccess.SERVICE_QUERY_STATUS);
        if (!service.IsInvalid)
        {
            return Task.FromResult(QueryServiceProcessId(serviceName, service));
        }

        var displayMatches = FindDisplayNameMatches(serviceName);

        if (displayMatches.Length > 1)
        {
            throw new DiagnosticsException(ExitCodes.UsageError, $"Multiple services matched display name '{serviceName}'. Use the exact service name instead.");
        }

        if (displayMatches.Length == 0)
        {
            throw new DiagnosticsException(ExitCodes.TargetNotFound, $"Windows service '{serviceName}' was not found.");
        }

        using var displayService = OpenService(manager, displayMatches[0].ServiceName, ServiceAccess.SERVICE_QUERY_STATUS);
        if (displayService.IsInvalid)
        {
            throw new DiagnosticsException(ExitCodes.CollectionFailed, $"Windows service '{displayMatches[0].ServiceName}' could not be opened for status queries.");
        }

        return Task.FromResult(QueryServiceProcessId(displayMatches[0].ServiceName, displayService));
    }

    private static int QueryServiceProcessId(string serviceName, SafeServiceHandle serviceHandle)
    {
        var bytesNeeded = 0;
        _ = QueryServiceStatusEx(serviceHandle, 0, IntPtr.Zero, 0, out bytesNeeded);
        var buffer = Marshal.AllocHGlobal(bytesNeeded);

        try
        {
            if (!QueryServiceStatusEx(serviceHandle, 0, buffer, bytesNeeded, out _))
            {
                throw new DiagnosticsException(ExitCodes.CollectionFailed, $"Failed to read status for Windows service '{serviceName}'. Win32 error: {Marshal.GetLastWin32Error()}.");
            }

            var status = Marshal.PtrToStructure<SERVICE_STATUS_PROCESS>(buffer);
            if (status.dwCurrentState == ServiceState.SERVICE_STOPPED)
            {
                throw new DiagnosticsException(ExitCodes.TargetNotFound, $"Windows service '{serviceName}' exists but is stopped.");
            }

            if (status.dwProcessId <= 0)
            {
                throw new DiagnosticsException(ExitCodes.CollectionFailed, $"Windows service '{serviceName}' is running but its PID could not be determined.");
            }

            return status.dwProcessId;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [SupportedOSPlatform("windows")]
    private static ServiceController[] FindDisplayNameMatches(string serviceName)
        => ServiceController.GetServices()
            .Where(service => string.Equals(service.DisplayName, serviceName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeServiceHandle OpenSCManager(IntPtr machineName, IntPtr databaseName, ServiceManagerAccess desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeServiceHandle OpenService(SafeServiceHandle serviceControlManager, string serviceName, ServiceAccess desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryServiceStatusEx(
        SafeServiceHandle service,
        int infoLevel,
        IntPtr buffer,
        int bufferSize,
        out int bytesNeeded);

    [StructLayout(LayoutKind.Sequential)]
    private struct SERVICE_STATUS_PROCESS
    {
        public ServiceType dwServiceType;
        public ServiceState dwCurrentState;
        public uint dwControlsAccepted;
        public uint dwWin32ExitCode;
        public uint dwServiceSpecificExitCode;
        public uint dwCheckPoint;
        public uint dwWaitHint;
        public int dwProcessId;
        public uint dwServiceFlags;
    }

    [Flags]
    private enum ServiceManagerAccess : uint
    {
        SC_MANAGER_CONNECT = 0x0001
    }

    [Flags]
    private enum ServiceAccess : uint
    {
        SERVICE_QUERY_STATUS = 0x0004
    }

    private enum ServiceType : uint
    {
        SERVICE_WIN32_OWN_PROCESS = 0x00000010,
        SERVICE_WIN32_SHARE_PROCESS = 0x00000020
    }

    private enum ServiceState : uint
    {
        SERVICE_STOPPED = 0x00000001
    }

    private sealed class SafeServiceHandle : SafeHandle
    {
        public SafeServiceHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
            => CloseServiceHandle(handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hScObject);
    }
}
