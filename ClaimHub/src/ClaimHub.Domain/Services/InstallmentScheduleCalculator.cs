using ClaimHub.Domain.Common;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Domain.Services;

public class InstallmentScheduleCalculator
{
    private const decimal MonthlySurchargeRate = 0.03m;

    public IReadOnlyList<Installment> Build(decimal annualPremium, PaymentFrequency frequency, DateOnly firstDueDate)
    {
        int count = InstallmentCount(frequency);
        int monthsBetween = 12 / count;
        decimal total = annualPremium * (1 + SurchargeRate(frequency));

        var schedule = new List<Installment>(count);
        for (int i = 0; i < count; i++)
        {
            decimal amount = MoneyMath.ToCents(total / count);
            schedule.Add(new Installment(i + 1, firstDueDate.AddMonths(i * monthsBetween), amount));
        }

        return schedule;
    }

    public static int InstallmentCount(PaymentFrequency frequency) => frequency switch
    {
        PaymentFrequency.Annual => 1,
        PaymentFrequency.Quarterly => 4,
        PaymentFrequency.Monthly => 12,
        _ => throw new DomainRuleException($"Unsupported payment frequency {frequency}.")
    };

    public static decimal SurchargeRate(PaymentFrequency frequency) =>
        frequency == PaymentFrequency.Monthly ? MonthlySurchargeRate : 0m;

    public decimal TotalWithSurcharge(decimal annualPremium, PaymentFrequency frequency) =>
        MoneyMath.ToCents(annualPremium * (1 + SurchargeRate(frequency)));
}
