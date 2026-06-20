namespace InventoryManagement;

public record SaleItem(
    string  ProductId,
    string  ProductName,
    string  Sku,
    int     QtyOrdered,
    int     QtyShipped,
    decimal UnitPrice,      // actual selling price (after discount)
    decimal UnitCost        // FIFO cost at time of sale (filled on shipment)
)
{
    public int Pending => QtyOrdered - QtyShipped;
    public decimal Cost => QtyShipped * UnitCost;
    public decimal Revenue => QtyShipped * UnitPrice;
    public decimal GrossProfit => Revenue - Cost;
    public decimal ProfitMargin => Revenue == 0 ? 0 : GrossProfit / Revenue;

    public override string ToString() =>
        $"  {Sku,-14} {ProductName,-26} Qty:{QtyOrdered,4}  " +
        $"@Price: ({UnitPrice,8:C})  COGS: ({UnitCost,8:C})  GP: ({GrossProfit,10:C})  " +
        $"@Margin: {ProfitMargin:P1}";
}

public record SalesOrder(
    string         OrderId,
    string         CustomerId,
    string         CustomerName,
    List<SaleItem> Items,
    SaleStatus     Status,
    PaymentStatus  PaymentStatus,
    decimal        DiscountRate,
    DateTime       CreatedAt,
    DateTime?      ShippedAt,
    DateTime?      DeliveredAt
)
{
    public decimal TotalCost => Items.Sum(l => l.Cost);
    public decimal GrossRevenue => Items.Sum(l => l.Revenue);
    public decimal GrossProfit => GrossRevenue - TotalCost;
    public decimal ProfitMargin => GrossRevenue == 0 ? 0 : GrossProfit / GrossRevenue;

    public override string ToString() =>
        $"[{OrderId}]  {CustomerName,-22}  {Status,-14}  " +
        $"TotalCost: ({TotalCost,10:C})  GrossRevenue: ({GrossRevenue,10:C})  " +
        $"GP: ({GrossProfit,10:C})  Margin: ({ProfitMargin:P1}) Payment: {PaymentStatus}";
}
