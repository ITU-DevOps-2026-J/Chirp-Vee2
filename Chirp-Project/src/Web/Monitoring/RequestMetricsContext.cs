using System.Threading;

namespace Web.Monitoring;

public static class RequestMetricsContext
{
    private static readonly AsyncLocal<string?> CurrentPath = new();

    public static string Path
    {
        get => CurrentPath.Value ?? "unknown";
        set => CurrentPath.Value = value;
    }
}
