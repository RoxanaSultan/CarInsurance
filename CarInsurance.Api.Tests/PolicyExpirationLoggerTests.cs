using System;
using CarInsurance.Api.BackgroundTasks;
using CarInsurance.Api.Models;
using Xunit;

namespace CarInsurance.Api.Tests;

public class PolicyExpirationLoggerTests
{
    private readonly PolicyExpirationLogger _logger;

    public PolicyExpirationLoggerTests()
    {
        _logger = new PolicyExpirationLogger(null!, null!);
    }

    [Fact]
    public void ShouldLogPolicy_ExactlyOneHourAfterExpiry_ReturnsTrue()
    {
        var policy = new InsurancePolicy
        {
            Id = 1,
            CarId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        var now = policy.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue).AddMinutes(30);

        var result = _logger.ShouldLogPolicy(policy, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldLogPolicy_BeforeExpiry_ReturnsFalse()
    {
        var policy = new InsurancePolicy
        {
            Id = 2,
            CarId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var now = DateTime.UtcNow;

        var result = _logger.ShouldLogPolicy(policy, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldLogPolicy_MoreThanOneHourAfterExpiry_ReturnsFalse()
    {
        var policy = new InsurancePolicy
        {
            Id = 3,
            CarId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        var now = policy.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(2);

        var result = _logger.ShouldLogPolicy(policy, now);

        Assert.False(result);
    }
}