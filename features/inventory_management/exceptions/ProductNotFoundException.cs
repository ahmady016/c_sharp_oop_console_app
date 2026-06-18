namespace InventoryManagement;

public sealed class ProductNotFoundException : InventoryException
{
    public ProductNotFoundException(string id) : base($"Product '{id}' Not Found.") { }
}
