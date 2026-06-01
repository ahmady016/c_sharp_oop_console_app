namespace HttpErrors;

public static class HttpErrorHandler
{
    public static void Handle(HttpException error)
    {
        Console.WriteLine();
        var color = error is HttpServerException
            ? ConsoleColor.Red
            : ConsoleColor.Yellow;
        Console.ForegroundColor = color;
        string errorType = error is HttpServerException
            ? "SERVER"
            : "CLIENT";
        Console.WriteLine($"  ┌─ {errorType} ERROR ──────────────────────────");
        Console.ResetColor();
        Console.WriteLine($"  │  Status  : {error.StatusCode}");
        Console.WriteLine($"  │  Message : {error.Message}");
        Console.WriteLine($"  │  TraceId : {error.TraceId}");
        Console.WriteLine($"  │  Retry   : {(error.IsRetryable ? "Yes" : "No")}");

        // extra context per type
        switch (error)
        {
            case BadRequestException bre when bre.Errors.Count > 0:
                Console.WriteLine($"  │  Errors  :");
                foreach (var (field, msgs) in bre.Errors)
                    Console.WriteLine($"  │    [{field}] {string.Join(", ", msgs)}");
                break;

            case UnauthorizedException uae:
                Console.WriteLine($"  │  Scheme  : {uae.AuthScheme ?? "unknown"}");
                break;

            case ForbiddenException fe when fe.RequiredPermission is not null:
                Console.WriteLine($"  │  Needs   : {fe.RequiredPermission} permission");
                break;

            case NotFoundException nfe:
                Console.WriteLine($"  │  Resource: {nfe.ResourceName}");
                break;

            case UnprocessableEntityException uee when uee.Reasons.Count > 0:
                Console.WriteLine($"  │  Reasons :");
                foreach (var r in uee.Reasons)
                    Console.WriteLine($"  │    • {r}");
                break;

            case BadGatewayException bge:
                Console.WriteLine($"  │  Upstream: {bge.UpstreamService}");
                Console.WriteLine($"  │  Retry in: ({bge.WaitSeconds})s");
                break;

            case HttpServerException sve:
                Console.WriteLine($"  │  Retry in: ({sve.WaitSeconds})s");
                break;
        }

        Console.WriteLine($"  └──────────────────────────────────────────");
    }

}
