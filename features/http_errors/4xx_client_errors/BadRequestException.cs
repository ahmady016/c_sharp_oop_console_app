namespace HttpErrors;

public sealed class BadRequestException : HttpClientException
{
    // carries the validation errors map —> [field → list of errors]
    private readonly IReadOnlyDictionary<string, string[]> _errorsMap;
    public IReadOnlyDictionary<string, string[]> Errors => _errorsMap;

    public BadRequestException(
        Dictionary<string, string[]>? errors,
        string message = "One or more validation errors occurred.",
        string? traceId = null
    ) : base(400, "Bad Request", message, traceId)
    {
        _errorsMap = (errors ?? []).AsReadOnly();
    }
}
