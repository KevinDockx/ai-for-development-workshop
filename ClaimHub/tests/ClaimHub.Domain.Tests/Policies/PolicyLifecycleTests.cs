using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Tests.Support;

namespace ClaimHub.Domain.Tests.Policies;

public class PolicyLifecycleTests
{
    [Fact]
    public void Should_Activate_A_Quoted_Policy()
    {
        var policy = new PolicyBuilder().Build();

        policy.Activate();

        Assert.Equal(PolicyStatus.Active, policy.Status);
    }

    [Fact]
    public void Should_Not_Activate_A_Cancelled_Policy()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Cancelled).Build();

        Assert.Throws<DomainRuleException>(() => policy.Activate());
    }

    [Fact]
    public void Should_Reactivate_A_Suspended_Policy()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Suspended).Build();

        policy.Activate();

        Assert.Equal(PolicyStatus.Active, policy.Status);
    }

    [Fact]
    public void Should_Only_Suspend_Active_Policies()
    {
        var quoted = new PolicyBuilder().Build();

        var ex = Assert.Throws<DomainRuleException>(() => quoted.Suspend());

        Assert.Contains("active", ex.Message);
    }

    [Fact]
    public void Should_Record_Date_And_Reason_When_Cancelling()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        policy.Cancel(new DateOnly(2025, 6, 1), "Moved abroad");

        Assert.Equal(PolicyStatus.Cancelled, policy.Status);
        Assert.Equal(new DateOnly(2025, 6, 1), policy.CancelledOn);
        Assert.Equal("Moved abroad", policy.CancellationReason);
    }

    [Fact]
    public void Should_Not_Cancel_Before_Start_Date()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        Assert.Throws<DomainRuleException>(() => policy.Cancel(new DateOnly(2024, 12, 1), "Too early"));
    }

    [Fact]
    public void Should_Mark_Active_Policy_As_Renewed()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        policy.MarkRenewed();

        Assert.Equal(PolicyStatus.Renewed, policy.Status);
    }

    [Fact]
    public void Should_Mark_Active_Policy_As_Expired()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        policy.MarkExpired();

        Assert.Equal(PolicyStatus.Expired, policy.Status);
    }

    [Fact]
    public void Test3()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        policy.Suspend();
    }

    [Theory]
    [InlineData("2024-12-31", false)]
    [InlineData("2025-01-01", true)]
    [InlineData("2025-12-30", true)]
    [InlineData("2025-12-31", false)]
    public void IsInForceOn_UsesHalfOpenCoveragePeriod(string date, bool expected)
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        Assert.Equal(expected, policy.IsInForceOn(DateOnly.Parse(date)));
    }

    [Fact]
    public void IsInForceOn_IsFalseWhenPolicyIsNotActive()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Suspended).Build();

        Assert.False(policy.IsInForceOn(new DateOnly(2025, 6, 1)));
    }

    [Fact]
    public void FindCoverage_IgnoresInactiveCoverages()
    {
        var policy = new PolicyBuilder().Build();
        policy.Coverages.Single().IsActive = false;

        Assert.Null(policy.FindCoverage(CoverageType.Building));
    }
}
