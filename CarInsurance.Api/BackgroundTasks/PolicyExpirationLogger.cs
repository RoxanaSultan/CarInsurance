using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.BackgroundTasks;

public class PolicyExpirationLogger : BackgroundService
{
    private readonly ILogger<PolicyExpirationLogger> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HashSet<long> _processedPolicies = new();

    public PolicyExpirationLogger(ILogger<PolicyExpirationLogger> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            var policies = await db.Policies.ToListAsync(stoppingToken);

            foreach (var policy in policies)
            {
                if (_processedPolicies.Contains(policy.Id))
                    continue;

                if (ShouldLogPolicy(policy, now))
                {
                    _logger.LogInformation(
                        "Policy {PolicyId} for Car {CarId} expired on {EndDate}",
                        policy.Id, policy.CarId, policy.EndDate
                    );

                    _processedPolicies.Add(policy.Id);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    public bool ShouldLogPolicy(InsurancePolicy policy, DateTime now)
    {
        var expirationMoment = policy.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return now >= expirationMoment && now <= expirationMoment.AddHours(1);
    }
}