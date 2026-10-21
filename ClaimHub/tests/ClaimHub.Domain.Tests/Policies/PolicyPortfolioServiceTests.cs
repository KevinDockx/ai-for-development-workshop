using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class PolicyPortfolioServiceTests
{
    private readonly PolicyPortfolioService _service = new();

    [Fact]
    public void Coverage_Summary_Lists_Active_Coverages_With_Full_Remaining_Limit()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var summary = _service.SummarizeCoverage(policy);

        summary.PolicyNumber.Should().Be("HOM-2025-000010");
        summary.Coverages.Should().ContainSingle();
        summary.Coverages[0].Type.Should().Be(CoverageType.Building);
        summary.Coverages[0].Remaining.Should().Be(300_000m);
        summary.TotalPaidInTerm.Should().Be(0m);
    }

    [Fact]
    public void Inactive_Coverages_Are_Left_Out_Of_The_Summary()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Coverages.First().IsActive = false;

        _service.SummarizeCoverage(policy).Coverages.Should().BeEmpty();
    }

    [Fact]
    public void Portfolio_Groups_Premium_By_Product()
    {
        var home = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        var motor = new PolicyBuilder().AsMotor().WithStatus(PolicyStatus.Active).WithAnnualPremium(400m).Build();
        motor.Id = 11;

        var summary = _service.Summarize(new[] { home, motor }, new DateOnly(2025, 6, 1));

        summary.PolicyCount.Should().Be(2);
        summary.InForceCount.Should().Be(2);
        summary.TotalAnnualPremium.Should().Be(1_000m);
        summary.ByProduct.Select(b => b.Key).Should().Equal("Home", "Motor");
    }

    [Fact]
    public void Portfolio_Ignores_Deleted_Policies()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.IsDeleted = true;

        var summary = _service.Summarize(new[] { policy }, new DateOnly(2025, 6, 1));

        summary.PolicyCount.Should().Be(0);
    }

    [Fact]
    public void Policy_Close_To_Its_End_Date_Is_Expiring_Soon()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var summary = _service.Summarize(new[] { policy }, new DateOnly(2025, 12, 15), 30);

        summary.ExpiringSoonCount.Should().Be(1);
        summary.ExpiringSoonPolicyIds.Should().Equal(10);
    }

    [Fact]
    public void Cancelled_Policy_Is_Never_Expiring()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Cancelled).Build();

        _service.IsExpiringWithin(policy, new DateOnly(2025, 12, 15), 30).Should().BeFalse();
    }

    [Fact]
    public void Negative_Expiring_Window_Is_Rejected()
    {
        var act = () => _service.Summarize(Array.Empty<ClaimHub.Domain.Entities.Policy>(), new DateOnly(2025, 6, 1), -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Renewal_Window_Opens_Sixty_Days_Before_The_End_Date()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var open = _service.GetRenewalWindow(policy, new DateOnly(2025, 11, 15));
        var closed = _service.GetRenewalWindow(policy, new DateOnly(2025, 10, 31));

        open.OpensOn.Should().Be(new DateOnly(2025, 11, 1));
        open.IsOpen.Should().BeTrue();
        open.DaysUntilEnd.Should().Be(46);
        closed.IsOpen.Should().BeFalse();
    }
}