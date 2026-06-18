namespace InventoryManagement;

public sealed class InsufficientStockException : InventoryException
{
    private readonly int _available;
    private readonly int _requested;
    public int Available => _available;
    public int Requested => _requested;

    public InsufficientStockException(
        string productId,
        int available,
        int requested
    )
        : base($"Insufficient stock for '{productId}'. Available: ({available}), Requested: ({requested}).")
    {
        _available = available;
        _requested = requested;
    }
}
