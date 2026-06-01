namespace HttpErrors;

public sealed class InternalServerErrorException : HttpServerException
{
    // retry is possible but timing is unknown so waitSeconds = 0
    public InternalServerErrorException(
        string message = "The server encountered an internal error and was unable to complete your request.",
        string? traceId = null,
        Exception? innerException = null
    ) : base(500, "Internal Server Error", message, 0, traceId, innerException) { }
}
