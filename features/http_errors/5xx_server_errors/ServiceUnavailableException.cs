namespace HttpErrors;

public sealed class ServiceUnavailableException : HttpServerException
{
    public ServiceUnavailableException(
        string message = "The server is currently unable to handle the request due to a temporary overload or scheduled maintenance.",
        int? waitSeconds = null,
        string? traceId = null,
        Exception? innerException = null
    ) : base(503, "Service Unavailable", message, waitSeconds ?? 60, traceId, innerException) { }
}
