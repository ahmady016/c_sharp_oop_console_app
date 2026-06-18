namespace InventoryManagement;

public sealed class CustomerNotFoundException : InventoryException
{
    public CustomerNotFoundException(string id) : base($"Customer '{id}' Not Found.") { }
}
