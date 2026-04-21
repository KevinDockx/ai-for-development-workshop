using ClaimHub.Domain.Entities;

namespace ClaimHub.Domain.Tests.Billing;

public class BillingTests
{
    [Fact]
    public void Invoice_Is_Due_Thirty_Days_After_Issue()
    {
        Assert.Equal(new DateOnly(2025, 2, 14), Invoice.DueDateFor(new DateOnly(2025, 1, 15)));
    }

    [Fact]
    public void Reminder_Date_Is_Fifteen_Days_After_Due_Date()
    {
        var invoice = new Invoice { DueDate = new DateOnly(2025, 2, 14) };

        Assert.Equal(new DateOnly(2025, 3, 1), invoice.ReminderDate);
    }
}
