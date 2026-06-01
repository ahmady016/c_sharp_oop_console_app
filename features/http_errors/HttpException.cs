using System.Diagnostics;

namespace HttpErrors;

/// <summary>
/// root of the HTTP exception hierarchy.
/// contains the HTTP status code, status text and a correlation trace ID.
/// enforce each HTTP exception to state IsRetryable getter only property
/// </summary>
public abstract class HttpException : Exception
{
    private readonly int _statusCode;
    private readonly string _statusText;
    private readonly string _traceId;

    public int StatusCode => _statusCode;
    public string StatusText => _statusText;
    public string TraceId => _traceId;

    public abstract bool IsRetryable { get; }

    protected HttpException(
        int statusCode,
        string statusText,
        string message,
        string? traceId,
        Exception? innerException = null
    ) : base(message, innerException)
    {
        _statusCode = statusCode;
        _statusText = statusText;
        _traceId = traceId ?? Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }

    public override string ToString() =>
        $"HTTP {StatusCode} {StatusText} Error, TraceId: {TraceId}";
}
