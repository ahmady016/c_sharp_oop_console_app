namespace HttpErrors;

/*
# mimic HTTP errors to practice OOP inheritance and exception handling
-----------------------------------------------------
# by building exceptions classes to handle popular HTTP errors [4xx, 5xx]
# Three-level hierarchy:
# Exception (.NET Base) → HttpException → 4xx/5xx base → specific error class.
# Each level adds exactly what its scope justifies as
-----------------------------------------------------
Exception -> [.NET base]
└── HttpException my root —> [StatusText + StatusCode + TraceId]
    ├── HttpClientException 4xx base — [IsRetryable = false]
        ├── BadRequestException 400
        ├── UnauthorizedException 401
        ├── ForbiddenException 403
        ├── NotFoundException 404
        ├── ConflictException 409
        ├── UnprocessableEntityException 422
    ├── HttpServerException 5xx base — [IsRetryable = true]
        ├── InternalServerErrorException 500
        ├── BadGatewayException 502
        ├── ServiceUnavailableException 503
        ├── GatewayTimeoutException 504
---------------------------------------------------------------------------
# The five best practices baked into this design
---------------------------------------------------------------------------
1. abstract root, sealed leaves.
    HttpException and the two mid-level bases (HttpClientException, HttpServerException)
    are abstract — they cannot be instantiated directly, only inherited.
    The concrete exceptions (NotFoundException, ServiceUnavailableException, etc.)
    are sealed — nobody can accidentally extend NotFoundException into something subtly different.
    This keeps the hierarchy flat and predictable.
2. Each level adds exactly one concern.
    HttpException adds StatusCode, StatusText and TraceId.
    HttpClientException adds IsRetryable = false.
    HttpServerException adds IsRetryable = true and WaitSeconds.
    Each specific class adds only the data its HTTP spec defines
    NotFoundException adds ResourceName and ResourceId, BadRequestException adds Errors.
    No class carries data that belongs to another level.
3. IsRetryable as an abstract property.
    Forcing every branch to declare its retry intent at the base level
    means your retry infrastructure — middleware, Polly policies, message queues
    can act on any HttpException without knowing its concrete type.
    This is OOP polymorphism solving a real problem.
4. Always chain innerException.
    Every constructor accepts an optional Exception? inner parameter and passes it to base().
    This preserves the original stack trace when you wrap a lower-level exception
    critical for debugging in production.
5. Global middleware catches the root,
    controllers catch the specific. catching HttpException in one middleware handles the 90% case
    log it, serialize it, return the right status code.
    individual controllers only need specific catch blocks
    when they need different behaviors for a particular error type.
    This is the open/closed principle in exception handling.
---------------------------------------------------------------------------
*/

// ════════════════════════════════════════════════════════════════════════════
//  TESTER MODELS
// ════════════════════════════════════════════════════════════════════════════
public record User(int Id, string Name, string Email, string Role);
public record Product(int Id, string Name, decimal Price, int Stock);
public record Order(int Id, int UserId, int ProductId, int Quantity);
// ════════════════════════════════════════════════════════════════════════════
//  FAKE REPOSITORY — throws HTTP exceptions to simulate real API behaviors
// ════════════════════════════════════════════════════════════════════════════
public sealed class UserRepository
{
    private readonly List<User> _users =
    [
        new(1, "Sara",  "sara@dev.io",  "admin"),
        new(2, "Omar",  "omar@dev.io",  "user"),
        new(3, "Layla", "layla@dev.io", "user"),
    ];
    private readonly HashSet<string> _bannedEmails = ["banned@spam.io"];
    private bool _tokenValid = true;
    public void InvalidateToken() => _tokenValid = false;
    public void RestoreToken() => _tokenValid = true;

    public User GetById(int id)
    {
        if (!_tokenValid)
            throw new UnauthorizedException(
                message: "Your session has expired. Please log in again.",
                authScheme: "Bearer"
            );
        return _users.FirstOrDefault(u => u.Id == id)
            ?? throw new NotFoundException(
                resourceName: "User",
                message: $"User with Id {id} not found."
            );
    }

    public User GetByIdByAdmin(int requestingUserId, int targetId)
    {
        var requester = _users.FirstOrDefault(u => u.Id == requestingUserId)
            ?? throw new NotFoundException(
                resourceName: "User",
                message: $"User with Id {requestingUserId} not found."
            );

        if (requester.Role != "admin")
            throw new ForbiddenException(
                message: $"User '{requester.Name}' cannot view other user profiles.",
                requiredPermission: "admin"
            );

        return _users.FirstOrDefault(u => u.Id == targetId)
            ?? throw new NotFoundException(
                resourceName: "User",
                message: $"User with Id {targetId} not found."
            );
    }

    public User Create(string name, string email)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name))
            errors["name"] = ["Name is required."];
        if (!email.Contains('@'))
            errors["email"] = ["Email address is invalid."];

        if (errors.Count > 0)
            throw new BadRequestException(errors: errors);

        if (_bannedEmails.Contains(email))
            throw new UnprocessableEntityException(
                reasons: ["This email address is not permitted."]
            );

        if (_users.Any(u => u.Email == email))
            throw new ConflictException(
                reasons: ["A user with email '{email}' already exists."],
                message: $"A user with email '{email}' already exists."
            );

        var user = new User(_users.Count + 1, name, email, "user");
        _users.Add(user);
        return user;
    }

}

public sealed class ProductRepository
{
    private readonly List<Product> _products =
    [
        new(1, "Laptop",   1200m, 5),
        new(2, "Mouse",      25m, 0),   // out of stock
        new(3, "Monitor",   450m, 3),
    ];
    // simulates an upstream payment gateway being down
    private bool _gatewayDown = false;
    public void SimulateGatewayDown()   => _gatewayDown = true;
    public void SimulateGatewayOnline() => _gatewayDown = false;

    public Product GetById(int id) => _products.FirstOrDefault(p => p.Id == id)
        ?? throw new NotFoundException("Product", $"Product with Id {id} not found.");

    public Order PlaceOrder(int userId, int productId, int quantity)
    {
        var product = GetById(productId);

        if (quantity <= 0)
            throw new BadRequestException(errors: new()
            {
                ["quantity"] = [$"Quantity must be at least 1, got {quantity}."]
            });

        if (product.Stock == 0)
            throw new UnprocessableEntityException(
                message: $"'{product.Name}' is out of stock.",
                reasons: ["Item has 0 units available.", "Check back later."]
            );

        if (product.Stock < quantity)
            throw new UnprocessableEntityException(
                message: $"Insufficient stock for '{product.Name}'.",
                reasons: [$"Requested: {quantity}", $"Available: {product.Stock}"]
            );

        if (_gatewayDown)
            throw new BadGatewayException(
                message: "Payment gateway did not respond.",
                upstreamService: "PaymentGateway",
                waitSeconds: 20
            );

        return new Order(
            Id: new Random().Next(1000, 9999),
            UserId: userId,
            ProductId: productId,
            Quantity: quantity
        );
    }


}
// ════════════════════════════════════════════════════════════════════════════
//  HTTP ERRORS TESTER
// ════════════════════════════════════════════════════════════════════════════
public class HttpErrorsManager
{
    static readonly UserRepository    Users    = new();
    static readonly ProductRepository Products = new();

    static void S1_GetExistingUser()
    {
        var user = Users.GetById(1);
        Helpers.PrintSuccess($"Found → {user.Name} <{user.Email}> [{user.Role}]");
    }

    static void S2_UserNotFound()
    {
        try   { Users.GetById(99); }
        catch (NotFoundException error) { HttpErrorHandler.Handle(error); }
    }

    static void S3_Unauthorized()
    {
        Users.InvalidateToken();
        try   { Users.GetById(1); }
        catch (UnauthorizedException error) { HttpErrorHandler.Handle(error); }
        finally { Users.RestoreToken(); }
    }

    // Omar (id=2, role=user) tries to read Layla's (id=3) profile
    static void S4_Forbidden()
    {
        try   { Users.GetByIdByAdmin(requestingUserId: 2, targetId: 3); }
        catch (ForbiddenException error) { HttpErrorHandler.Handle(error); }
    }

    static void S5_BadRequest()
    {
        try   { Users.Create(name: "", email: "not-an-email"); }
        catch (BadRequestException error) { HttpErrorHandler.Handle(error); }
    }

    static void S6_Conflict()
    {
        try   { Users.Create("Sara Duplicate", "sara@dev.io"); }
        catch (ConflictException error) { HttpErrorHandler.Handle(error); }
    }

    static void S7_Unprocessable()
    {
        try   { Users.Create("Spammer", "banned@spam.io"); }
        catch (UnprocessableEntityException error) { HttpErrorHandler.Handle(error); }
    }

    static void S8_SuccessfulOrder()
    {
        var order = Products.PlaceOrder(userId: 1, productId: 1, quantity: 2);
        Helpers.PrintSuccess(
            $"Order #{order.Id} placed → Product {order.ProductId} x{order.Quantity}"
        );
    }

    // try to order a Mouse (id=2) (stock=0)
    static void S9_OutOfStock()
    {
        try   { Products.PlaceOrder(userId: 2, productId: 2, quantity: 1); }
        catch (UnprocessableEntityException error) { HttpErrorHandler.Handle(error); }
    }

    static void S10_InvalidQuantity()
    {
        try   { Products.PlaceOrder(userId: 1, productId: 3, quantity: -5); }
        catch (BadRequestException error) { HttpErrorHandler.Handle(error); }
    }

    static void S11_GatewayDown()
    {
        Products.SimulateGatewayDown();
        try   { Products.PlaceOrder(userId: 1, productId: 3, quantity: 1); }
        catch (BadGatewayException error) { HttpErrorHandler.Handle(error); }
        finally { Products.SimulateGatewayOnline(); }
    }

    // catch HttpClientException (handles any 4xx generically)
    static void S12_CatchByBase()
    {
        var ids = new[] { 1, 42, 100 };
        foreach (var id in ids)
        {
            try
            {
                var user = Users.GetById(id);
                Helpers.PrintSuccess($"  User {id} → {user.Name}");
            }
            catch (HttpClientException error)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(
                    $"  ↳ Caught by base HttpClientException: {error.StatusCode} — {error.Message}"
                );
                Console.ResetColor();
            }
        }
    }

    public static void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string title = "HTTP ERRORS DEMO";
        Helpers.PrintHeader(title);
        Helpers.RunScenario("1 — 200 OK: Get existing user",                  S1_GetExistingUser);
        Helpers.RunScenario("2 — 404 Not Found: Get non-existent user",       S2_UserNotFound);
        Helpers.RunScenario("3 — 401 Unauthorized: Expired session",          S3_Unauthorized);
        Helpers.RunScenario("4 — 403 Forbidden: Non-admin reads other user",  S4_Forbidden);
        Helpers.RunScenario("5 — 400 Bad Request: Invalid user registration", S5_BadRequest);
        Helpers.RunScenario("6 — 409 Conflict: Duplicate email",              S6_Conflict);
        Helpers.RunScenario("7 — 422 Unprocessable: Banned email",            S7_Unprocessable);
        Helpers.RunScenario("8 — 200 OK: Successful order",                   S8_SuccessfulOrder);
        Helpers.RunScenario("9 — 422 Unprocessable: Out of stock",            S9_OutOfStock);
        Helpers.RunScenario("10 — 400 Bad Request: Invalid quantity",         S10_InvalidQuantity);
        Helpers.RunScenario("11 — 502 Bad Gateway: Payment gateway down",     S11_GatewayDown);
        Helpers.RunScenario("12 — Catch by base type: any 4xx",               S12_CatchByBase);
        Helpers.PrintFooter(title);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }
}
