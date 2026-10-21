using ClaimHub.Domain.Services;
using ClaimHub.Infrastructure.Services;
using ClaimHub.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClaimHub.Infrastructure.Data;

/// <summary>
/// Creates the shared SQLite database, applies migrations and seeds demo data when the database is empty.
/// </summary>
public class ClaimHubDatabaseInitializer
{
    private readonly ClaimHubDbContext _context;
    private readonly IDocumentStorage _documentStorage;
    private readonly ILogger<ClaimHubDatabaseInitializer> _logger;

    public ClaimHubDatabaseInitializer(
        ClaimHubDbContext context,
        IDocumentStorage documentStorage,
        ILogger<ClaimHubDatabaseInitializer> logger)
    {
        _context = context;
        _documentStorage = documentStorage;
        _logger = logger;
    }

    private const string InMemoryDataSource = ":memory:";
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(200);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        string databasePath = ResolveDatabasePath();

        // In-memory databases are private to this connection, so there is nothing to share or lock.
        await using FileStream initializationLock = databasePath == null
            ? null
            : await AcquireInitializationLockAsync(databasePath + ".init.lock", cancellationToken);

        await _context.Database.MigrateAsync(cancellationToken);

        if (await _context.PolicyHolders.AnyAsync(cancellationToken) || await _context.Users.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Database already contains data; seeding skipped.");
            return;
        }

        await SeedAsync(cancellationToken);
    }

    private string ResolveDatabasePath()
    {
        string dataSource = _context.Database.GetDbConnection().DataSource;
        if (string.IsNullOrEmpty(dataSource) || dataSource == InMemoryDataSource)
        {
            return null;
        }

        string fullPath = Path.GetFullPath(dataSource);
        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return fullPath;
    }

    // API, Admin and Worker can start at the same time against a missing database. An exclusive file handle
    // serializes migration and seeding across processes; the empty lock file is left in place on purpose
    // because deleting it could let a second process lock a different file while others are still waiting.
    private async Task<FileStream> AcquireInitializationLockAsync(string lockPath, CancellationToken cancellationToken)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        bool waitLogged = false;
        while (true)
        {
            try
            {
                return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                if (timer.Elapsed >= LockTimeout)
                {
                    throw new TimeoutException(
                        $"Timed out after {LockTimeout.TotalSeconds:0} seconds waiting for the database initialization lock '{lockPath}'.", ex);
                }

                if (!waitLogged)
                {
                    _logger.LogInformation("Waiting for another process to finish database initialization.");
                    waitLogged = true;
                }

                await Task.Delay(LockRetryDelay, cancellationToken);
            }
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        var seed = ClaimHubSeedData.Generate();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.Brokers.AddRange(seed.Brokers);
        _context.Users.AddRange(seed.Users);
        _context.PolicyHolders.AddRange(seed.PolicyHolders);
        _context.Policies.AddRange(seed.Policies);
        _context.Claims.AddRange(seed.Claims);
        _context.Invoices.AddRange(seed.Invoices);
        _context.Payments.AddRange(seed.Payments);
        _context.RatingTable.AddRange(seed.RatingTable);
        _context.NumberSequences.AddRange(BuildSequenceRows(seed));
        await _context.SaveChangesAsync(cancellationToken);

        seed.CompleteAfterInsert();
        _context.AuditEntries.AddRange(seed.AuditEntries);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await WritePlaceholderDocumentsAsync(seed, cancellationToken);

        _logger.LogInformation(
            "Seeded {Holders} policyholders, {Policies} policies, {Claims} claims and {Invoices} invoices.",
            seed.PolicyHolders.Count, seed.Policies.Count, seed.Claims.Count, seed.Invoices.Count);
    }

    private static IEnumerable<NumberSequence> BuildSequenceRows(ClaimHubSeedData seed)
    {
        // Start each sequence at the highest seeded number so new numbers continue after the demo data.
        var policyRows = seed.Policies
            .Select(p => PolicyNumberGenerator.TryParse(p.PolicyNumber, out var product, out int year, out int sequence)
                ? (Name: DatabasePolicyNumberSequence.SequenceName(product, year), Value: sequence)
                : default)
            .Where(r => r.Name != null);

        var claimRows = seed.Claims
            .Select(c => c.ClaimNumber.Split('-'))
            .Select(parts => (Name: DatabaseClaimNumberSequence.SequenceName(int.Parse(parts[1])), Value: int.Parse(parts[2])));

        var invoiceRows = seed.Invoices
            .Select(i => i.InvoiceNumber.Split('-'))
            .Select(parts => (Name: DatabaseInvoiceNumberSequence.SequenceName(parts[0], int.Parse(parts[1])), Value: int.Parse(parts[2])));

        return policyRows.Concat(claimRows).Concat(invoiceRows)
            .GroupBy(r => r.Name)
            .Select(g => new NumberSequence { Name = g.Key, LastValue = g.Max(r => r.Value) });
    }

    private async Task WritePlaceholderDocumentsAsync(ClaimHubSeedData seed, CancellationToken cancellationToken)
    {
        foreach (var claim in seed.Claims)
        {
            foreach (var document in claim.Documents)
            {
                using var content = new MemoryStream(ClaimHubSeedData.PlaceholderContent(claim.ClaimNumber, document.FileName));
                await _documentStorage.SaveAsync(claim.ClaimNumber, document.FileName, content, cancellationToken);
            }
        }
    }
}
