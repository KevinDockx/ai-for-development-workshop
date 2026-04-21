using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Domain.Tests.Support;
using FluentAssertions;

namespace ClaimHub.Domain.Tests.Policies;

public class PremiumAndDunningProjectionTests
{
    private readonly PremiumExplanationService _explanations = new();
    private readonly DunningScheduleService _dunning = new();
    private readonly PolicyHolderOverviewService _overviews = new();

    [Fact]
    public void Premium_Explanation_Starts_With_The_Base_Premium_And_Ends_At_The_Annual_Premium()
    {
        var quote = new PremiumCalculator().Calculate(TestData.HomeRequest());

        var explanation = _explanations.Explain(quote);

        explanation.Lines.First().Kind.Should().Be(PremiumStepKind.Base);
        explanation.Lines.First().Amount.Should().Be(307.50m);
        explanation.Lines.Single(l => l.Kind == PremiumStepKind.Tax).Amount.Should().Be(28.44m);
        explanation.AnnualPremium.Should().Be(335.94m);
    }

    [Theory]
    [InlineData("2025-02-20", DunningStage.NotYetDue)]
    [InlineData("2025-03-10", DunningStage.Due)]
    [InlineData("2025-03-20", DunningStage.ReminderDue)]
    [InlineData("2025-04-20", DunningStage.SuspensionDue)]
    public void Invoice_Stage_Follows_The_Reminder_And_Suspension_Dates(string today, DunningStage expected)
    {
        var invoice = new Invoice { Id = 5, PolicyId = 10, Status = InvoiceStatus.Issued, Amount = 80m, DueDate = new DateOnly(2025, 3, 1) };

        var projection = _dunning.Project(invoice, DateOnly.Parse(today));

        projection.Stage.Should().Be(expected);
        projection.ReminderDate.Should().Be(new DateOnly(2025, 3, 16));
        projection.SuspensionDate.Should().Be(new DateOnly(2025, 4, 15));
    }

    [Fact]
    public void Quarterly_Policy_Projects_Four_Numbered_Installments()
    {
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.PaymentFrequency = PaymentFrequency.Quarterly;

        var schedule = _dunning.ProjectSchedule(policy);

        schedule.Select(s => s.Number).Should().Equal(1, 2, 3, 4);
        schedule.Should().AllSatisfy(s => s.ReminderDate.Should().Be(s.DueDate.AddDays(15)));
    }

    [Fact]
    public void Holder_Overview_Counts_Policies_In_Force_And_Next_Expiry()
    {
        var holder = TestData.PolicyHolder();
        holder.Policies.Add(new PolicyBuilder().WithStatus(PolicyStatus.Active).Build());

        var overview = _overviews.Build(holder, new DateOnly(2025, 6, 1));

        overview.InForceCount.Should().Be(1);
        overview.TotalAnnualPremium.Should().Be(600m);
        overview.NextExpiry.Should().Be(new DateOnly(2025, 12, 31));
        overview.Tier.Should().Be(CustomerTier.Loyal);
    }

    [Fact]
    public void Vip_Holder_Ranks_Above_Long_Standing_Tenure()
    {
        var holder = TestData.PolicyHolder();
        holder.IsVip = true;

        _overviews.Build(holder, new DateOnly(2025, 6, 1)).Tier.Should().Be(CustomerTier.Vip);
    }

    [Fact]
    public void Holder_Over_The_Credit_Limit_Gets_A_Note()
    {
        var holder = TestData.PolicyHolder();
        holder.CreditLimit = 50m;
        var policy = new PolicyBuilder().WithStatus(PolicyStatus.Active).Build();
        policy.Invoices.Add(new Invoice { Id = 7, PolicyId = 10, Status = InvoiceStatus.Overdue, Amount = 120m, DueDate = new DateOnly(2025, 3, 1) });
        holder.Policies.Add(policy);

        var overview = _overviews.Build(holder, new DateOnly(2025, 6, 1));

        overview.IsOverCreditLimit.Should().BeTrue();
        overview.Notes.Should().ContainSingle(n => n.Contains("credit limit"));
    }
}