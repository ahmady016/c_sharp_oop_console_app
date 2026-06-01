namespace HttpErrors;
public sealed class UnauthorizedException : HttpClientException
{
    private readonly string _authScheme = "Bearer";
    public string AuthScheme => _authScheme;

    public UnauthorizedException(
        string message = "Authentication is required.",
        string? authScheme = null,
        string? traceId = null
    ) : base(401, "Unauthorized", message, traceId)
    {
        if(!string.IsNullOrEmpty(authScheme?.Trim()))
            _authScheme = authScheme.Trim();
    }
}
