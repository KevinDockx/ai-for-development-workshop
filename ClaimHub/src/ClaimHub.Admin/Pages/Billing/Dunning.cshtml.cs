using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages.Billing;

public class DunningModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly DunningScheduleService _schedule;

    public DunningModel(ClaimHubDbContext db, DunningScheduleService schedule)
    {
        _db = db;
        _schedule = schedule;
    }

    public DateOnly Today { get; private set; }

    public IReadOnlyList<DunningRow> Rows { get; private set; } = Array.Empty<DunningRow>();

    public DunningOverview Overview { get; private set; } = new();

    public IReadOnlyDictionary<int, InvoiceDunningProjection> Projections { get; private set; } = new Dictionary<int, InvoiceDunningProjection>();
    public int ReminderCount => Rows.Count(r => r.Stage == "Reminder due");

    public int SuspensionCount => Rows.Count(r => r.Stage == "Suspension due");

    public decimal TotalOutstanding => Rows.Sum(r => r.Outstanding);

    public async Task OnGetAsync()
    {
        Today = BelgianTime.Today();

        var invoices = await _db.Invoices
            .Include(i => i.Policy).ThenInclude(p => p!.PolicyHolder)
            .Include(i => i.Payments)
            .Where(i => (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.Overdue) && i.DueDate < Today)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        Rows = invoices
            .Select(i => new DunningRow(
                i,
                Today.DayNumber - i.DueDate.DayNumber,
                i.OutstandingAmount,
                StageFor(i)))
            .Where(r => r.Outstanding > 0m)
            .ToList();

        Overview = _schedule.Summarize(Rows.Select(r => r.Invoice), Today);
        Projections = Rows.ToDictionary(r => r.Invoice.Id, r => _schedule.Project(r.Invoice, Today));
    }

    private string StageFor(Invoice invoice)
    {
        if (Today >= invoice.SuspensionDate)
        {
            return "Suspension due";
        }

        return Today >= invoice.ReminderDate ? "Reminder due" : "Grace period";
    }

    public record DunningRow(Invoice Invoice, int DaysOverdue, decimal Outstanding, string Stage);
}
