namespace OnlineStore.Domain;

/// <summary>
/// Represents a violation of an online store domain business rule.
/// </summary>
public class DomainRuleViolationException : Exception
{
    public string RuleCode { get; }

    public DomainRuleViolationException(string ruleCode, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleCode);
        RuleCode = ruleCode;
    }

    public DomainRuleViolationException(string ruleCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleCode);
        RuleCode = ruleCode;
    }
}
