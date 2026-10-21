using ClaimHub.Domain.Common;

namespace ClaimHub.Domain.Models;

public sealed record ValidationIssue(string Field, string Message);

public sealed class ValidationReport
{
    private readonly List<ValidationIssue> _issues = new();

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    public bool IsValid => _issues.Count == 0;

    public ValidationReport Add(string field, string message)
    {
        _issues.Add(new ValidationIssue(field, message));
        return this;
    }

    public ValidationReport AddIf(bool condition, string field, string message) =>
        condition ? Add(field, message) : this;

    public ValidationReport Merge(ValidationReport other)
    {
        ArgumentNullException.ThrowIfNull(other);
        _issues.AddRange(other.Issues);
        return this;
    }

    public IEnumerable<string> MessagesFor(string field) =>
        _issues.Where(i => string.Equals(i.Field, field, StringComparison.OrdinalIgnoreCase)).Select(i => i.Message);

    public void ThrowIfInvalid()
    {
        if (!IsValid)
        {
            throw new DomainRuleException(string.Join(" ", _issues.Select(i => $"{i.Field}: {i.Message}")));
        }
    }
}
