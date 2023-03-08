namespace ClaimHub.Domain.Common;

public class DomainRuleException : Exception
{
    public DomainRuleException(string message)
        : base(message)
    {
    }

    public DomainRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
