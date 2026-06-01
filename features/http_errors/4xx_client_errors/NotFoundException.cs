namespace HttpErrors;

public sealed class NotFoundException : HttpClientException
{
    private readonly string _resourceName;
    public string ResourceName => _resourceName;

    public NotFoundException(
        string resourceName,
        string message = "The requested resource was not found.",
        string? traceId = null
    ) : base(404, "Not Found", message, traceId)
    {
        if (string.IsNullOrEmpty(resourceName))
            throw new ArgumentNullException(nameof(resourceName));
        _resourceName = resourceName;
    }
}
