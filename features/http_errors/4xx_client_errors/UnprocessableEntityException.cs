namespace HttpErrors;

public sealed class UnprocessableEntityException : HttpClientException
{
    private readonly IReadOnlyList<string> _reasons;
    public IReadOnlyList<string> Reasons => _reasons;

    public UnprocessableEntityException(
        string message = "The request was well-formed but was unable to be followed due to semantic errors.",
        List<string>? reasons = null,
        string? traceId = null
    ) : base(422, "Unprocessable Entity", message, traceId)
    {
        _reasons = (IReadOnlyList<string>?)reasons?.AsReadOnly() ?? [];
    }
}
