namespace A2G.ServiceStats;

internal static class ExitCodes
{
    public const int Success = 0;
    public const int UsageError = 2;
    public const int TargetNotFound = 3;
    public const int PlatformNotSupported = 4;
    public const int TargetNotAttachable = 5;
    public const int PermissionDenied = 6;
    public const int Timeout = 7;
    public const int CollectionFailed = 8;
    public const int UnexpectedError = 99;
}
