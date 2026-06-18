namespace InventoryManagement;

public sealed class SupplierNotFoundException : InventoryException
{
    public SupplierNotFoundException(string id) : base($"Supplier '{id}' Not Found.") { }
}
