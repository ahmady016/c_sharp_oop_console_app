namespace HttpErrors;

public sealed class ConflictException : HttpClientException
{
    private readonly IReadOnlyList<string> _reasons = [];
    public IReadOnlyList<string> Reasons => _reasons;

    public ConflictException(
        List<string>? reasons,
        string message = "The request could not be completed due to a conflict with the current state of the resource.",
        string? traceId = null
    ) : base(409, "Conflict", message, traceId)
    {
        if(reasons is not null && reasons.Count > 0)
            _reasons = reasons.AsReadOnly();
    }
}
