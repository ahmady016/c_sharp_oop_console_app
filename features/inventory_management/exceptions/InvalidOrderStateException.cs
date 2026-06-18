namespace InventoryManagement;

public sealed class InvalidOrderStateException : InventoryException
{
    public InvalidOrderStateException(string msg) : base(msg) { }
}
