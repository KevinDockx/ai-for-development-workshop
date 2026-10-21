using System;
using System.Threading;
using System.Threading.Tasks;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Reporting;

/// <summary>Loads the open invoices once and lets the domain aging service group them.</summary>
public class ReceivablesQuery
{
    private readonly ClaimHubDbContext _db;
    private readonly InvoiceAgingService _aging;

    public ReceivablesQuery(ClaimHubDbContext db, InvoiceAgingService aging)
    {
        _db = db;
        _aging = aging;
    }

    public async Task<ReceivablesAging> GetAgingAsync(DateOnly today, CancellationToken cancellationToken)
    {
        var invoices = await _db.Invoices.AsNoTracking()
            .Include(i => i.Policy)
            .Include(i => i.Payments)
            .Where(i => i.Status == Domain.Enums.InvoiceStatus.Issued || i.Status == Domain.Enums.InvoiceStatus.Overdue)
            .ToListAsync(cancellationToken);

        return _aging.Build(invoices, today);
    }

    public async Task<ReceivablesAging> GetAgingForHolderAsync(int policyHolderId, DateOnly today, CancellationToken cancellationToken)
    {
        var invoices = await _db.Invoices.AsNoTracking()
            .Include(i => i.Policy)
            .Include(i => i.Payments)
            .Where(i => i.Policy.PolicyHolderId == policyHolderId
                && (i.Status == Domain.Enums.InvoiceStatus.Issued || i.Status == Domain.Enums.InvoiceStatus.Overdue))
            .ToListAsync(cancellationToken);

        return _aging.Build(invoices, today);
    }
}