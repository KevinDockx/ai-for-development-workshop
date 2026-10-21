using System.Globalization;
using ClaimHub.Contracts.Billing;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using ClaimHub.Infrastructure.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ClaimHub.Api.Controllers;

[ApiController]
[Route("api/v1/billing")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly ClaimHubDbContext _db;
    private readonly IPaymentGateway _gateway;
    private readonly CreditNoteService _creditNotes;
    private readonly IInvoiceNumberSequence _invoiceNumbers;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        ClaimHubDbContext db,
        IPaymentGateway gateway,
        CreditNoteService creditNotes,
        IInvoiceNumberSequence invoiceNumbers,
        ILogger<BillingController> logger)
    {
        _db = db;
        _gateway = gateway;
        _creditNotes = creditNotes;
        _invoiceNumbers = invoiceNumbers;
        _logger = logger;
    }

    [HttpGet("invoices")]
    [ProducesResponseType<IEnumerable<InvoiceDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListInvoices(
        [FromQuery] int? policyId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (page < 1)
        {
            return BadRequest("page must be 1 or greater");
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return BadRequest("pageSize must be between 1 and 200");
        }

        var query = _db.Invoices.AsNoTracking().Include(i => i.Payments).Include(i => i.Policy).AsQueryable();
        if (policyId.HasValue)
        {
            query = query.Where(i => i.PolicyId == policyId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var invoices = await query
            .OrderBy(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = invoices.Select(i => new InvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            PolicyId = i.PolicyId,
            PolicyNumber = i.Policy?.PolicyNumber,
            InstallmentNumber = i.InstallmentNumber,
            InstallmentCount = i.InstallmentCount,
            Amount = i.Amount,
            PaidAmount = i.PaidTotal,
            OutstandingAmount = i.OutstandingAmount,
            IssuedOn = i.IssuedOn,
            DueDate = i.DueDate,
            Status = i.Status,
            IsCreditNote = i.IsCreditNote
        });

        return Ok(items);
    }

    [HttpGet("invoices/{id:int}")]
    [ProducesResponseType<InvoiceDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound($"Invoice {id} does not exist");
        }

        return Ok(new InvoiceDetailDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            PolicyId = invoice.PolicyId,
            InstallmentNumber = invoice.InstallmentNumber,
            InstallmentCount = invoice.InstallmentCount,
            Amount = invoice.Amount,
            PaidAmount = invoice.PaidTotal,
            OutstandingAmount = invoice.OutstandingAmount,
            IssuedOn = invoice.IssuedOn,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            IsCreditNote = invoice.IsCreditNote,
            CreditedInvoiceId = invoice.CreditedInvoiceId,
            PaidAt = invoice.PaidAt,
            Payments = invoice.Payments.Select(p => new InvoicePaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Direction = p.Direction,
                Method = p.Method,
                Status = p.Status,
                GatewayReference = p.GatewayReference,
                CompletedAt = p.CompletedAt
            }).ToList()
        });
    }

    [HttpPost("invoices/{id:int}/payments")]
    public async Task<IActionResult> RegisterPayment(int id, [FromBody] RegisterPaymentRequest request)
    {
        if (request == null)
        {
            return BadRequest("A request body is required");
        }

        if (request.Amount <= 0m)
        {
            return BadRequest("Amount must be greater than zero");
        }

        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .Include(i => i.Policy).ThenInclude(p => p!.PolicyHolder)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound($"Invoice {id} does not exist");
        }

        if (invoice.IsCreditNote)
        {
            return BadRequest("Credit notes cannot be paid");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return BadRequest("Invoice is cancelled");
        }

        if (request.Amount > invoice.OutstandingAmount)
        {
            return BadRequest($"Amount exceeds the outstanding balance of {invoice.OutstandingAmount}");
        }

        var now = DateTimeOffset.UtcNow;
        var payment = Payment.ForInvoice(invoice, request.Amount, request.Method, now);
        payment.IdempotencyKey = request.IdempotencyKey;

        var result = await _gateway.SubmitAsync(new PaymentGatewayRequest(
            PaymentDirection.Incoming,
            payment.Amount,
            invoice.InvoiceNumber,
            invoice.Policy?.PolicyHolder?.Iban ?? string.Empty,
            request.IdempotencyKey));

        if (result.Succeeded)
        {
            payment.Complete(result.GatewayReference, result.ProcessedAt);
        }
        else
        {
            payment.Fail(result.FailureReason ?? "declined");
        }

        invoice.Payments.Add(payment);
        if (result.Succeeded && invoice.OutstandingAmount <= 0m)
        {
            invoice.MarkPaid(now);
        }

        await _db.SaveChangesAsync();

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, payment.FailureReason);
        }

        return Ok(new PaymentDto
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status,
            GatewayReference = payment.GatewayReference,
            CompletedAt = payment.CompletedAt,
            InvoiceOutstanding = invoice.OutstandingAmount
        });
    }

    [HttpPost("invoices/{id:int}/refund")]
    [AllowAnonymous]
    public async Task<IActionResult> Refund(int id, [FromBody] RefundRequest request)
    {
        if (request == null)
        {
            return BadRequest("A request body is required");
        }

        if (request.Amount <= 0m)
        {
            return BadRequest("Refund amount must be greater than zero");
        }

        var original = await _db.Invoices
            .Include(i => i.Payments)
            .Include(i => i.Policy).ThenInclude(p => p!.PolicyHolder)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (original == null)
        {
            return NotFound($"Invoice {id} does not exist");
        }

        if (original.Status != InvoiceStatus.Paid)
        {
            return BadRequest("Only paid invoices can be refunded");
        }

        var existingNotes = await _db.Invoices
            .Where(i => i.CreditedInvoiceId == original.Id)
            .ToListAsync();

        var report = _creditNotes.Validate(original, request.Amount, existingNotes);
        if (!report.IsValid)
        {
            return BadRequest(string.Join("; ", report.Issues.Select(i => i.Message)));
        }

        var note = _creditNotes.Issue(original, request.Amount, existingNotes, DateOnly.FromDateTime(DateTime.UtcNow));
        note.InvoiceNumber = await _invoiceNumbers.NextAsync(InvoiceNumberKinds.CreditNote, note.IssuedOn.Year);

        var result = await _gateway.SubmitAsync(new PaymentGatewayRequest(
            PaymentDirection.Outgoing,
            request.Amount,
            note.InvoiceNumber,
            original.Policy?.PolicyHolder?.Iban ?? string.Empty,
            $"refund-{original.Id}-{note.InvoiceNumber}"));
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status502BadGateway, result.FailureReason);
        }

        _db.Invoices.Add(note);
        await _db.SaveChangesAsync();

        return Ok(new RefundResultDto
        {
            InvoiceId = original.Id,
            CreditNoteNumber = note.InvoiceNumber,
            RefundedAmount = request.Amount,
            GatewayReference = result.GatewayReference
        });
    }

    [HttpPost("payments/import")]
    [Authorize(Roles = "Admin,Underwriter")]
    public async Task<IActionResult> ImportPayments()
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest("Empty import file");
        }

        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var batch = JsonConvert.DeserializeObject<PaymentImportBatch>(body, settings);
        if (batch == null || batch.Rows == null)
        {
            return BadRequest("Import file could not be read");
        }

        var outcome = new PaymentImportResult { BatchReference = batch.BatchReference };
        foreach (var row in batch.Rows)
        {
            if (string.IsNullOrEmpty(row.InvoiceNumber))
            {
                outcome.Skipped++;
                outcome.Errors.Add("Row without invoice number");
                continue;
            }

            if (!decimal.TryParse(row.Amount, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount) || amount <= 0m)
            {
                outcome.Skipped++;
                outcome.Errors.Add($"{row.InvoiceNumber}: invalid amount '{row.Amount}'");
                continue;
            }

            var invoice = await _db.Invoices.Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == row.InvoiceNumber);
            if (invoice == null)
            {
                outcome.Skipped++;
                outcome.Errors.Add($"{row.InvoiceNumber}: unknown invoice");
                continue;
            }

            if (invoice.IsCreditNote || amount > invoice.OutstandingAmount)
            {
                outcome.Skipped++;
                outcome.Errors.Add($"{row.InvoiceNumber}: amount does not fit the open balance");
                continue;
            }

            var at = row.PaidOn ?? DateTimeOffset.UtcNow;
            var payment = Payment.ForInvoice(invoice, amount, row.Method, at);
            payment.Complete($"IMPORT-{batch.BatchReference}", at);
            invoice.Payments.Add(payment);
            if (invoice.OutstandingAmount <= 0m)
            {
                invoice.MarkPaid(at);
            }

            outcome.Imported++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Imported {Count} payments from batch {Batch}", outcome.Imported, batch.BatchReference);
        return Ok(outcome);
    }
}
