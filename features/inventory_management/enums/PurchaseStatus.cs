namespace InventoryManagement;

public enum PurchaseStatus : byte
{
    Draft = 1,
    Submitted = 2,
    PartialReceived = 3,
    FullyReceived = 4,
    Cancelled = 5
}
