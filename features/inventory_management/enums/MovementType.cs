namespace InventoryManagement;

public enum MovementType : byte
{
    PurchaseIn = 1,
    SaleOut = 2,
    Adjustment = 3,
    Return = 4,
    Transfer = 5
}
