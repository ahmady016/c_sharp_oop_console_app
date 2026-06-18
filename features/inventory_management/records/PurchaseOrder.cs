namespace InventoryManagement;

public record PurchaseItem(
    string  ProductId,
    string  ProductName,
    string  Sku,
    int     QtyOrdered,
    int     QtyReceived,
    decimal UnitCost
)
{
    public int Pending => QtyOrdered - QtyReceived;
    public decimal Subtotal => QtyOrdered * UnitCost;
    public override string ToString() =>
        $"  {Sku,-14} {ProductName,-26} Qty:{QtyOrdered,4}  | " +
        $"Received:{QtyReceived,4}  @{UnitCost,8:C}  Sub:{Subtotal,10:C}";
}

public record PurchaseOrder(
    string           OrderId,
    string           SupplierId,
    string           SupplierName,
    List<PurchaseItem> Lines,
    PurchaseStatus   Status,
    PaymentStatus    PaymentStatus,
    DateTime         CreatedAt,
    DateTime?        ReceivedAt
)
{
    public decimal TotalCost => Lines.Sum(l => l.Subtotal);
    public decimal ReceivedCost => Lines.Sum(l => l.QtyReceived * l.UnitCost);
    public override string ToString() =>
        $"[{OrderId}]  {SupplierName,-22}  {Status,-16}  " +
        $"Total:{TotalCost,10:C}  Payment:{PaymentStatus}";
}
