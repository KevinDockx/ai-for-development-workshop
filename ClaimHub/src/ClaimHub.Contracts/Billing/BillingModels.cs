using System.ComponentModel.DataAnnotations;
using ClaimHub.Domain.Enums;

namespace ClaimHub.Contracts.Billing;

public class InvoiceDto
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public int PolicyId { get; set; }

    public string? PolicyNumber { get; set; }

    public int InstallmentNumber { get; set; }

    public int InstallmentCount { get; set; }

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal OutstandingAmount { get; set; }

    public DateOnly IssuedOn { get; set; }

    public DateOnly DueDate { get; set; }

    public InvoiceStatus Status { get; set; }

    public bool IsCreditNote { get; set; }
}

public class InvoiceDetailDto : InvoiceDto
{
    public int? CreditedInvoiceId { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    public List<InvoicePaymentDto> Payments { get; set; } = new();
}

public class InvoicePaymentDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public PaymentDirection Direction { get; set; }

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; }

    public string? GatewayReference { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

public class RegisterPaymentRequest
{
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;

    public string? IdempotencyKey { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }

    public int? InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; }

    public string? GatewayReference { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public decimal InvoiceOutstanding { get; set; }
}

public class RefundRequest
{
    public decimal Amount { get; set; }

    public string? Reason { get; set; }
}

public class RefundResultDto
{
    public int InvoiceId { get; set; }

    public string CreditNoteNumber { get; set; } = string.Empty;

    public decimal RefundedAmount { get; set; }

    public string? GatewayReference { get; set; }
}

public class PaymentImportRow
{
    public string InvoiceNumber { get; set; } = string.Empty;

    // Bank exports deliver amounts as text, for example "1.234,56" or "98.40".
    public string Amount { get; set; } = string.Empty;

    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;

    public DateTimeOffset? PaidOn { get; set; }
}

public class PaymentImportBatch
{
    public string BatchReference { get; set; } = string.Empty;

    public List<PaymentImportRow> Rows { get; set; } = new();
}

public class PaymentImportResult
{
    public string BatchReference { get; set; } = string.Empty;

    public int Imported { get; set; }

    public int Skipped { get; set; }

    public List<string> Errors { get; set; } = new();
}
