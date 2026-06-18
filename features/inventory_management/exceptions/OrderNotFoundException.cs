namespace InventoryManagement;

public sealed class OrderNotFoundException : InventoryException
{
    public OrderNotFoundException(string id) : base($"Order '{id}' Not Found.") { }
}
