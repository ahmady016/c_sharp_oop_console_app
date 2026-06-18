namespace InventoryManagement;

public sealed class DuplicateSkuException : InventoryException
{
    public DuplicateSkuException(string sku)
        : base($"Product SKU '{sku}' already exists.") { }
}
