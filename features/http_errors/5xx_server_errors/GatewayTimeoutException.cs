namespace HttpErrors;

public sealed class GatewayTimeoutException : HttpServerException
{
    private readonly int _timeoutSeconds = 30;
    public int TimeoutSeconds => _timeoutSeconds;

    public GatewayTimeoutException(
        string message = "The server did not respond in time.",
        int? timeoutSeconds = null,
        int? waitSeconds = null,
        string? traceId = null,
        Exception? innerException = null
        ) : base(504, "Gateway Timeout", message, waitSeconds ?? 30, traceId, innerException)
    {
        if(timeoutSeconds is not null && timeoutSeconds > 0)
            _timeoutSeconds = timeoutSeconds.Value;
    }
}
