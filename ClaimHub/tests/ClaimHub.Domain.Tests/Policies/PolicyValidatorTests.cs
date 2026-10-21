using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Rules;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class PolicyValidatorTests
{
    private readonly PolicyValidator _validator = new();
    private readonly DateOnly _today = new(2025, 3, 1);

    [Fact]
    public void Should_Accept_A_Complete_Home_Policy()
    {
        var report = _validator.Validate(new PolicyBuilder().Build(), _today);

        report.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Accept_A_Complete_Motor_Policy()
    {
        var report = _validator.Validate(new PolicyBuilder().AsMotor().Build(), _today);

        report.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Reject_End_Date_Before_Start_Date()
    {
        var policy = new PolicyBuilder().WithTerm(new DateOnly(2025, 6, 1), new DateOnly(2025, 1, 1)).Build();

        var report = _validator.Validate(policy, _today);

        report.MessagesFor("EndDate").Should().ContainSingle();
    }

    [Fact]
    public void Should_Reject_Active_Policy_Past_Its_End_Date()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var report = _validator.Validate(policy, new DateOnly(2026, 2, 1));

        report.IsValid.Should().BeFalse();
        report.MessagesFor("Status").Should().ContainSingle();
    }

    [Fact]
    public void Should_Require_Reason_For_Cancelled_Policy()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Cancelled).Build();
        policy.CancelledOn = new DateOnly(2025, 2, 1);

        var report = _validator.Validate(policy, _today);

        report.MessagesFor("CancellationReason").Should().NotBeEmpty();
    }

    [Fact]
    public void Should_Reject_Motor_Policy_Without_Liability_Coverage()
    {
        var policy = new PolicyBuilder().AsMotor(withLiability: false).Build();

        var report = _validator.Validate(policy, _today);

        report.MessagesFor("Coverages").Should().Contain(m => m.Contains("ThirdPartyLiability"));
    }

    [Fact]
    public void Should_Reject_Policy_Without_Policyholder()
    {
        var policy = new PolicyBuilder().Build();
        policy.PolicyHolderId = 0;

        _validator.Validate(policy, _today).IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsReadyForActivation_Is_True_For_Valid_Quote()
    {
        _validator.IsReadyForActivation(new PolicyBuilder().Build(), _today).Should().BeTrue();
    }

    [Fact]
    public void IsReadyForActivation_Is_False_For_Active_Policy()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        _validator.IsReadyForActivation(policy, _today).Should().BeFalse();
    }

    [Fact]
    public void Test1()
    {
        var policy = new PolicyBuilder().AsMotor().Build();

        var report = _validator.Validate(policy, _today);

        report.ThrowIfInvalid();
    }
}
