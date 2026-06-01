namespace HttpErrors;

/// <summary>
/// root of the [HTTP 4xx client errors] exception hierarchy.
/// 4xx are the caller's fault so IsRetryable = false
/// retrying the same bad request will always fail
/// </summary>
public abstract class HttpClientException : HttpException
{
    public override bool IsRetryable => false;
    protected HttpClientException(
        int statusCode,
        string statusText,
        string message,
        string? traceId,
        Exception? innerException = null
    ) : base(statusCode, statusText, message, traceId, innerException) { }
}
