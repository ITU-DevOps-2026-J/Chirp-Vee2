using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prometheus;

namespace Web.Monitoring;

public sealed class DbCommandMetricsInterceptor : DbCommandInterceptor
{
    private static readonly Histogram DbCommandDuration = Metrics.CreateHistogram(
        "chirp_db_command_duration_seconds",
        "Database command duration grouped by request path and command kind.",
        new HistogramConfiguration
        {
            LabelNames = ["path", "command_kind"],
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 14)
        });

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Observe("reader", eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        Observe("scalar", eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        Observe("non_query", eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    private static void Observe(string commandKind, TimeSpan duration)
    {
        var path = RequestMetricsContext.Path;
        DbCommandDuration.WithLabels(path, commandKind).Observe(duration.TotalSeconds);
    }
}
