namespace HttpErrors;

public sealed class BadGatewayException : HttpServerException
{
    private readonly string _upstreamService = "";
    public string UpstreamService => _upstreamService;

    public BadGatewayException(
        string message = "The server was acting as a gateway or proxy and received an invalid response from the upstream server.",
        string upstreamService = "",
        int? waitSeconds = null,
        string? traceId = null,
        Exception? innerException = null
    ) : base(502, "Bad Gateway", message, waitSeconds ?? 15, traceId, innerException)
    {
        if(!string.IsNullOrEmpty(upstreamService?.Trim()))
            _upstreamService = upstreamService.Trim();
    }
}
