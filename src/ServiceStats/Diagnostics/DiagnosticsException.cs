namespace A2G.ServiceStats.Diagnostics;

internal sealed class DiagnosticsException : Exception
{
    public DiagnosticsException(int exitCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}
