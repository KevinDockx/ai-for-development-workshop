namespace ClaimHub.Worker.Options;

/// <summary>
/// Scheduling settings shared by every job. Production runs use long intervals; the Development
/// settings shorten them so all jobs fire within a few seconds of startup.
/// </summary>
public class JobScheduleOptions
{
    public bool Enabled { get; set; } = true;

    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMinutes(1);

    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Run a single time after the initial delay instead of repeating.</summary>
    public bool RunOnce { get; set; }
}

public class BillingRunJobOptions : JobScheduleOptions
{
    public const string SectionName = "Jobs:Billing";

    public BillingRunJobOptions()
    {
        InitialDelay = TimeSpan.FromMinutes(5);
        Interval = TimeSpan.FromHours(24);
    }

    /// <summary>Installments falling due within this many days are invoiced.</summary>
    public int LookaheadDays { get; set; } = 30;

    public int MaxPoliciesPerRun { get; set; } = 1000;
}

public class DunningJobOptions : JobScheduleOptions
{
    public const string SectionName = "Jobs:Dunning";

    public DunningJobOptions()
    {
        InitialDelay = TimeSpan.FromMinutes(10);
        Interval = TimeSpan.FromHours(6);
    }

    public int ReminderAfterDays { get; set; } = 15;

    /// <summary>The agreed rule in the billing policy: suspend cover 45 days after the due date.</summary>
    public int SuspensionAfterDays { get; set; } = 45;

    public int MaxInvoicesPerRun { get; set; } = 500;
}

public class ClaimSlaEscalationJobOptions : JobScheduleOptions
{
    public const string SectionName = "Jobs:ClaimSla";

    public ClaimSlaEscalationJobOptions()
    {
        InitialDelay = TimeSpan.FromMinutes(2);
        Interval = TimeSpan.FromMinutes(30);
    }

    public int EscalationAfterDays { get; set; } = 5;
}

public class RenewalJobOptions : JobScheduleOptions
{
    public const string SectionName = "Jobs:Renewal";

    public RenewalJobOptions()
    {
        InitialDelay = TimeSpan.FromMinutes(15);
        Interval = TimeSpan.FromHours(12);
    }

    /// <summary>Policies ending within this many days are renewed.</summary>
    public int RenewalWindowDays { get; set; } = 45;

    public int MaxRenewalsPerRun { get; set; } = 25;

    public string RunAsUser { get; set; } = "worker@scheldt.example";
}

public class NightlyExportJobOptions : JobScheduleOptions
{
    public const string SectionName = "Jobs:NightlyExport";

    public NightlyExportJobOptions()
    {
        InitialDelay = TimeSpan.FromMinutes(20);
        Interval = TimeSpan.FromHours(24);
    }

    /// <summary>Relative paths are resolved against the folder that contains ClaimHub.sln.</summary>
    public string ExportFolder { get; set; } = "exports";

    public string FilePrefix { get; set; } = "claimhub-export";
}

public class JobClockOptions
{
    public const string SectionName = "Clock";

    /// <summary>When set, every job treats this date as today. Leave empty to use the system clock.</summary>
    public DateOnly? AsOfDate { get; set; }
}