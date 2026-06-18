namespace InventoryManagement;

public abstract class InventoryException : Exception
{
    protected InventoryException(string message, Exception? inner = null)
        : base(message, inner) { }
}
