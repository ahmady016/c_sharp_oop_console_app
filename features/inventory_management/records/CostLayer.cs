namespace InventoryManagement;

// FIFO cost layer — tracks a batch of stock at a specific purchase cost
public record CostLayer(
    string   PurchaseOrderId,
    int      QuantityRemaining,
    decimal  UnitCost,
    DateTime PurchasedAt
)
{
    public override string ToString()
        => $"[{PurchasedAt:yyyy-MM-dd hh:mm tt}] -> {QuantityRemaining} @ {UnitCost}";
}
