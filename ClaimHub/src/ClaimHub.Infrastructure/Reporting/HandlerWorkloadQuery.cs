using System;
using System.Threading;
using System.Threading.Tasks;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Models;
using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Infrastructure.Reporting;

/// <summary>Loads active staff and open claims and lets the domain service spread the claims over the handlers.</summary>
public class HandlerWorkloadQuery
{
    private readonly ClaimHubDbContext _db;
    private readonly HandlerWorkloadService _workload;

    public HandlerWorkloadQuery(ClaimHubDbContext db, HandlerWorkloadService workload)
    {
        _db = db;
        _workload = workload;
    }

    public async Task<HandlerWorkloadReport> GetReportAsync(DateOnly today, CancellationToken cancellationToken)
    {
        var handlers = await _db.Users.AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        var openClaims = await _db.Claims.AsNoTracking()
            .Where(c => !c.IsDeleted
                && (c.Status == Domain.Enums.ClaimStatus.Reported || c.Status == Domain.Enums.ClaimStatus.UnderAssessment))
            .ToListAsync(cancellationToken);

        return _workload.Build(handlers, openClaims, today);
    }
}