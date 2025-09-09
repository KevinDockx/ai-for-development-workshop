namespace ClaimHub.Domain.Models;

public class SlaAssessment
{
    public int ClaimId { get; set; }

    public DateOnly ReportedOn { get; set; }

    public DateOnly Deadline { get; set; }

    public int ElapsedDays { get; set; }

    public int AllowedDays { get; set; }

    public bool IsAssessed { get; set; }

    public bool IsBreached { get; set; }

    public bool RequiresEscalation { get; set; }
}
