namespace HttpErrors;

public sealed class ForbiddenException : HttpClientException
{
    private readonly string _requiredPermission = "Access Permission";
    public string RequiredPermission => _requiredPermission;

    public ForbiddenException(
        string message = "You do not have permission to access this resource.",
        string? requiredPermission = null,
        string? traceId = null
    ) : base(403, "Forbidden", message, traceId)
    {
        if(!string.IsNullOrEmpty(requiredPermission?.Trim()))
            _requiredPermission = requiredPermission.Trim();
    }
}
