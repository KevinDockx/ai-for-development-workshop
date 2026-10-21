using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class PolicyChangeDecisionServiceTests
{
    private readonly PolicyChangeDecisionService _service = new();

    [Fact]
    public void Cancellation_Summary_Carries_The_Calculator_Refund()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        var effectiveOn = new DateOnly(2025, 7, 1);

        var summary = _service.DescribeCancellation(policy, effectiveOn);

        summary.Refund.RefundAmount.Should().Be(new CancellationRefundCalculator().Calculate(policy, effectiveOn).RefundAmount);
        summary.PolicyNumber.Should().Be(policy.PolicyNumber);
    }

    [Fact]
    public void Cancellation_Summary_Only_Cancels_Open_Invoices_Due_After_The_Effective_Date()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        var invoices = new[]
        {
            Invoice(1, InvoiceStatus.Issued, new DateOnly(2025, 8, 1)),
            Invoice(2, InvoiceStatus.Paid, new DateOnly(2025, 8, 1)),
            Invoice(3, InvoiceStatus.Overdue, new DateOnly(2025, 6, 1))
        };

        var summary = _service.DescribeCancellation(policy, new DateOnly(2025, 7, 1), invoices);

        summary.InvoiceIdsToCancel.Should().Equal(1);
        summary.OutstandingOnOpenInvoices.Should().Be(100m);
    }

    [Theory]
    [InlineData(900, PremiumDirection.Additional)]
    [InlineData(600, PremiumDirection.NoChange)]
    [InlineData(300, PremiumDirection.Credit)]
    public void Endorsement_Summary_Reports_The_Direction_Of_The_Premium_Change(int newPremium, PremiumDirection expected)
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();

        var summary = _service.DescribeEndorsement(policy, newPremium, new DateOnly(2025, 7, 1));

        summary.Direction.Should().Be(expected);
        summary.RemainingShare.Should().BeInRange(0m, 1m);
    }

    [Fact]
    public void Endorsement_Delta_Adds_Up_All_Recorded_Endorsements()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Endorsements.Add(new PolicyEndorsement { Id = 1, PremiumDelta = 25m, EffectiveDate = new DateOnly(2025, 3, 1) });
        policy.Endorsements.Add(new PolicyEndorsement { Id = 2, PremiumDelta = -10m, EffectiveDate = new DateOnly(2025, 5, 1) });

        _service.TotalEndorsementDelta(policy).Should().Be(15m);
        _service.EndorsementsInTerm(policy).Select(e => e.Id).Should().Equal(1, 2);
    }

    private static Invoice Invoice(int id, InvoiceStatus status, DateOnly dueDate) =>
        new() { Id = id, PolicyId = 10, Status = status, Amount = 100m, DueDate = dueDate };
}