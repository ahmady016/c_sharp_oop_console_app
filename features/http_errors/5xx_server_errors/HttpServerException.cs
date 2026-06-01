namespace HttpErrors;
/// <summary>
/// root of the [HTTP 5xx server errors] exception hierarchy.
/// 5xx are the server's fault so IsRetryable = true
/// so the same request may succeed after a brief wait.
/// </summary>
public abstract class HttpServerException : HttpException
{
    private readonly int _waitSeconds = 0;
    public int WaitSeconds => _waitSeconds;

    public override bool IsRetryable => true;
    protected HttpServerException(
        int statusCode,
        string statusText,
        string message,
        int? waitSeconds,
        string? traceId,
        Exception? innerException = null
    ) : base(statusCode, statusText, message, traceId, innerException)
    {
        if(waitSeconds is not null && waitSeconds > 0)
            _waitSeconds = waitSeconds.Value;
    }
}
