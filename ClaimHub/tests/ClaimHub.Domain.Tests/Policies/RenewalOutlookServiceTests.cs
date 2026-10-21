using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class RenewalOutlookServiceTests
{
    private readonly RenewalOutlookService _service = new();

    private static Claim PaidClaim(decimal paid) =>
        new() { Status = ClaimStatus.PaidOut, PaidAmount = paid, DateOfLoss = TestData.NewYear2025.AddDays(30) };

    [Fact]
    public void Policy_Without_Claims_Can_Renew_As_Is()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var outlook = _service.Evaluate(policy, TestData.NewYear2025.AddMonths(10));

        outlook.Advice.Should().Be(RenewalAdvice.RenewAsIs);
        outlook.LossRatioPercent.Should().Be(0m);
        outlook.HasFindings.Should().BeFalse();
    }

    [Fact]
    public void High_Loss_Ratio_Triggers_A_Premium_Review()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Claims.Add(PaidClaim(400m));

        var outlook = _service.Evaluate(policy, TestData.NewYear2025.AddMonths(10));

        outlook.LossRatioPercent.Should().Be(66.7m);
        outlook.Advice.Should().Be(RenewalAdvice.ReviewPremium);
        outlook.Findings.Should().Contain(f => f.Code == "LOSS_RATIO_HIGH");
    }

    [Fact]
    public void Loss_Ratio_Above_Premium_Needs_Manual_Underwriting()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Claims.Add(PaidClaim(700m));

        var outlook = _service.Evaluate(policy, TestData.NewYear2025.AddMonths(10));

        outlook.Advice.Should().Be(RenewalAdvice.ManualUnderwriting);
    }

    [Fact]
    public void Rejected_Claims_Do_Not_Count_Towards_The_Outlook()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Claims.Add(new Claim { Status = ClaimStatus.Rejected, PaidAmount = 900m });

        var outlook = _service.Evaluate(policy, TestData.NewYear2025.AddMonths(10));

        outlook.ClaimCount.Should().Be(0);
        outlook.ClaimCost.Should().Be(0m);
    }

    [Fact]
    public void Two_Claims_Flag_Claim_Frequency_Even_With_A_Low_Loss_Ratio()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Claims.Add(PaidClaim(50m));
        policy.Claims.Add(PaidClaim(60m));

        var outlook = _service.Evaluate(policy, TestData.NewYear2025.AddMonths(10));

        outlook.Advice.Should().Be(RenewalAdvice.ReviewPremium);
        outlook.Findings.Should().Contain(f => f.Code == "CLAIM_FREQUENCY");
    }

    [Fact]
    public void Suspended_Policy_Is_Never_Renewed_Without_Review()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Suspended).Build();

        var outlook = _service.Evaluate(policy, TestData.NewYear2025.AddMonths(10));

        outlook.Advice.Should().Be(RenewalAdvice.ManualUnderwriting);
        outlook.Findings.Should().ContainSingle(f => f.Code == "SUSPENDED");
    }

    [Fact]
    public void Statement_Running_Balance_Follows_Invoices_Credits_And_Payments()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        var invoice = new Invoice { InvoiceNumber = "INV-1", Amount = 600m, IssuedOn = TestData.NewYear2025, Status = InvoiceStatus.Issued };
        invoice.Payments.Add(new Payment
        {
            Amount = 200m,
            Status = PaymentStatus.Completed,
            CompletedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
        });
        policy.Invoices.Add(invoice);

        var statement = new AccountStatementService().Build(1, new[] { policy });

        statement.Entries.Should().HaveCount(2);
        statement.TotalInvoiced.Should().Be(600m);
        statement.Balance.Should().Be(400m);
        statement.IsSettled.Should().BeFalse();
    }
}