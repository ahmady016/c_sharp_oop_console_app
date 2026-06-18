namespace InventoryManagement;

public record StockMovement(
    string       MovementId,
    string       ProductId,
    MovementType Type,
    int          Quantity,
    decimal      UnitCost,   // what it cost us (purchase price)
    decimal      UnitPrice,  // what we sold it for (sale price)
    string       Warehouse,
    string       Reference,
    DateTime     Timestamp,
    string       PerformedBy
)
{
    public decimal TotalCost => Quantity * UnitCost;
    public decimal TotalPrice => Quantity * UnitPrice;
    public decimal GrossProfit => TotalPrice - TotalCost;

    public override string ToString() =>
        $"  {Timestamp:yyyy-MM-dd hh:mm tt}  {Type,-14} {Quantity,+6}  " +
        $"Cost: {UnitCost,8:C}  Price: {UnitPrice,8:C}  Ref: {Reference}";
}
