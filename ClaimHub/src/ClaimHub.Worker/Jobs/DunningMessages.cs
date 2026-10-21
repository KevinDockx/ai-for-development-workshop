using ClaimHub.Domain.Entities;
using ClaimHub.Infrastructure.External;

namespace ClaimHub.Worker.Jobs;

internal static class DunningMessages
{
    public static EmailMessage Reminder(PolicyHolder holder, Policy policy, Invoice invoice) =>
        new(
            holder.Email,
            $"Payment reminder for invoice {invoice.InvoiceNumber}",
            $"Dear {holder.FullName}, invoice {invoice.InvoiceNumber} for policy {policy.PolicyNumber} "
            + $"({invoice.OutstandingAmount:0.00} EUR) was due on {invoice.DueDate:dd/MM/yyyy} and is still open. "
            + "Please pay it as soon as possible to keep your cover in force.");

    public static EmailMessage Suspension(PolicyHolder holder, Policy policy, Invoice invoice) =>
        new(
            holder.Email,
            $"Your policy {policy.PolicyNumber} has been suspended",
            $"Dear {holder.FullName}, because invoice {invoice.InvoiceNumber} ({invoice.OutstandingAmount:0.00} EUR, "
            + $"due {invoice.DueDate:dd/MM/yyyy}) remains unpaid, your policy {policy.PolicyNumber} is suspended. "
            + "Pay the open amount and your cover will be reinstated.");
}