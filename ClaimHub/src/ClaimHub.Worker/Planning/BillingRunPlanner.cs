using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Worker.Scheduling;

namespace ClaimHub.Worker.Planning;

public sealed record PendingInvoice(Policy Policy, Installment Installment, int InstallmentCount);

public sealed class BillingRunPlan
{
    public BillingRunPlan(int policiesExamined, IReadOnlyList<PendingInvoice> pending, int skipped, DateOnly horizon)
    {
        PoliciesExamined = policiesExamined;
        Pending = pending;
        Skipped = skipped;
        Horizon = horizon;
    }

    public int PoliciesExamined { get; }

    public IReadOnlyList<PendingInvoice> Pending { get; }

    public int Skipped { get; }

    public DateOnly Horizon { get; }

    public bool IsEmpty => Pending.Count == 0;

    public decimal PlannedAmount => Pending.Sum(p => p.Installment.Amount);

    public JobRunResult EmptyResult() =>
        new(PoliciesExamined, 0, Skipped, $"no installments due up to {Horizon:yyyy-MM-dd}");

    public JobRunResult Result(decimal invoicedTotal, IReadOnlyList<string> numbers) =>
        new(PoliciesExamined, Pending.Count, Skipped, $"invoiced {invoicedTotal:0.00} EUR, numbers {numbers[0]} to {numbers[^1]}");
}

/// <summary>Works out which installments the billing run has to invoice, without touching the database.</summary>
public sealed class BillingRunPlanner
{
    private readonly InstallmentScheduleCalculator _installments;

    public BillingRunPlanner(InstallmentScheduleCalculator installments)
    {
        _installments = installments;
    }

    public BillingRunPlan Plan(IReadOnlyList<Policy> policies, DateOnly today, DateOnly horizon)
    {
        var pending = new List<PendingInvoice>();
        int skipped = 0;

        foreach (var policy in policies)
        {
            var due = FindInstallmentsDue(policy, today, horizon);
            if (due.Count == 0)
            {
                skipped++;
                continue;
            }

            pending.AddRange(due);
        }

        return new BillingRunPlan(policies.Count, pending, skipped, horizon);
    }

    public IReadOnlyList<PendingInvoice> FindInstallmentsDue(Policy policy, DateOnly today, DateOnly horizon)
    {
        var result = new List<PendingInvoice>();
        if (policy.AnnualPremium <= 0m)
        {
            return result;
        }

        var schedule = _installments.Build(policy.AnnualPremium, policy.PaymentFrequency, policy.StartDate);
        foreach (var installment in schedule)
        {
            if (installment.DueDate < today || installment.DueDate > horizon)
            {
                continue;
            }

            if (installment.DueDate >= policy.EndDate)
            {
                continue;
            }

            if (policy.CancelledOn.HasValue && installment.DueDate >= policy.CancelledOn.Value)
            {
                continue;
            }

            result.Add(new PendingInvoice(policy, installment, schedule.Count));
        }

        return result;
    }
}