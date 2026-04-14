using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace Web.Monitoring;

public sealed class SystemMetricsCollector(
    IServiceScopeFactory scopeFactory,
    ILogger<SystemMetricsCollector> logger) : BackgroundService
{
    private static readonly Gauge RegisteredUsers = Metrics.CreateGauge(
        "chirp_registered_users",
        "Current number of registered users in the Author table.");

    private static readonly Gauge AverageFollowersPerUser = Metrics.CreateGauge(
        "chirp_average_followers_per_user",
        "Current average number of followers per user.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateGauges(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to update system gauges.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task UpdateGauges(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

        var followerLists = await dbContext.Authors
            .AsNoTracking()
            .Select(a => a.Follows)
            .ToListAsync(stoppingToken);

        var registeredUsers = followerLists.Count;
        var averageFollowers = registeredUsers == 0
            ? 0
            : followerLists.Average(follows => follows?.Count ?? 0);

        RegisteredUsers.Set(registeredUsers);
        AverageFollowersPerUser.Set(averageFollowers);
    }
}
