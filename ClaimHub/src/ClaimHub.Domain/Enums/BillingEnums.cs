namespace ClaimHub.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    CreditNote = 5
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Reversed = 3
}

public enum PaymentMethod
{
    DirectDebit = 1,
    BankTransfer = 2,
    Card = 3
}

public enum PaymentDirection
{
    Incoming = 1,
    Outgoing = 2
}
