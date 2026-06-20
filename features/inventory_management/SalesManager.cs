namespace InventoryManagement;

public interface ISalesManager
{
    IReadOnlyList<SalesOrder> All { get; }

    SalesOrder Create(
        string customerId,
        string customerName,
        IEnumerable<(string ProductId, string Name, string Sku, int Quantity, decimal UnitPrice)> items,
        decimal discountRate
    );
    SalesOrder Cancel(string orderId);
    SalesOrder Ship(string orderId, string warehouse, string by);
    SalesOrder Deliver(string orderId);
    SalesOrder MarkPaid(string orderId);
    SalesOrder Refund(string orderId, string warehouse, string by);
    SalesOrder FindById(string orderId);
    IReadOnlyList<SalesOrder> FindByStatus(SaleStatus status);
}

public sealed class SalesManager : ISalesManager
{
    private int _seq = 0;
    private readonly IStockLedger _ledger;
    private readonly Dictionary<string, SalesOrder> _ordersMap = [];
    public IReadOnlyList<SalesOrder> All => [..
        _ordersMap.Values.OrderByDescending(o => o.CreatedAt)
    ];

    public SalesManager(IStockLedger ledger) => _ledger = ledger;

    public SalesOrder Create(
        string customerId,
        string customerName,
        IEnumerable<(string ProductId, string Name, string Sku, int Quantity, decimal UnitPrice)> items,
        decimal discountRate
    )
    {
        var saleItems = items
            .Select(item => new SaleItem(
                ProductId: item.ProductId,
                ProductName: item.Name,
                Sku: item.Sku,
                QtyOrdered: item.Quantity,
                QtyShipped: 0,
                UnitPrice: item.UnitPrice,
                UnitCost: 0m
            ))
            .ToList();
        var newSaleOrder = new SalesOrder(
            OrderId:      $"SO-{++_seq:D5}",
            CustomerId:   customerId,
            CustomerName: customerName,
            Items:        saleItems,
            Status:       SaleStatus.Confirmed,
            PaymentStatus: PaymentStatus.Unpaid,
            DiscountRate: discountRate,
            CreatedAt:    DateTime.Now,
            ShippedAt:    null,
            DeliveredAt:  null
        );
        _ordersMap[newSaleOrder.OrderId] = newSaleOrder;
        return newSaleOrder;
    }

    public SalesOrder Cancel(string orderId)
    {
        // get the existed sale order and ensure it is not already shipped or delivered
        var existedOrder = FindById(orderId);
        if (existedOrder.Status is SaleStatus.Shipped or SaleStatus.Delivered)
            throw new InvalidOrderStateException($"Cannot cancel Sale Order ({orderId}) — already {existedOrder.Status}.");

        // mark the sale order as cancelled
        var updatedOrder = existedOrder with { Status = SaleStatus.Cancelled };

        // update the ordersMap with the updated order and return it
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public SalesOrder Ship(string orderId, string warehouse, string by)
    {
        // get the existed sale order and ensure it is confirmed to be shipped
        var existedOrder = FindById(orderId);
        if (existedOrder.Status != SaleStatus.Confirmed)
            throw new InvalidOrderStateException($"Sale Order ({orderId}) must be Confirmed to be shipped. Current Status: {existedOrder.Status}");

        // do stockOut operation and capture actual cost
        // and update the shipped quantity and unit cost in each sale item
        var shippedItems = existedOrder.Items.Select(item =>
        {
            decimal fifoCost = _ledger.StockOut(
                productId: item.ProductId,
                quantity: item.QtyOrdered,
                warehouse: warehouse,
                reference: orderId,
                by: by
            );
            return item with { QtyShipped = item.QtyOrdered, UnitCost = fifoCost };
        }).ToList();

        // update the sale order with shipped items and mark it as shipped
        var updatedOrder = existedOrder with
        {
            Items     = shippedItems,
            Status    = SaleStatus.Shipped,
            ShippedAt = DateTime.Now
        };
        // update the ordersMap with the updated order and return it
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public SalesOrder Deliver(string orderId)
    {
        // get the existed sale order and ensure it is shipped to be delivered
        var existedOrder = FindById(orderId);
        if (existedOrder.Status != SaleStatus.Shipped)
            throw new InvalidOrderStateException($"Sale Order ({orderId}) must be Shipped to deliver.");

        // mark the sale order as delivered and update the delivered date
        var updatedOrder = existedOrder with
        {
            Status      = SaleStatus.Delivered,
            DeliveredAt = DateTime.Now
        };

        // update the ordersMap with the updated order and return it
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public SalesOrder MarkPaid(string orderId)
    {
        // get the existed sale order and ensure it is not already paid
        var existedOrder = FindById(orderId);
        if(existedOrder.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOrderStateException($"Sale Order ({orderId}) is already paid.");

        // mark the sale order as paid
        var updatedOrder = existedOrder with { PaymentStatus = PaymentStatus.Paid };

        // update the ordersMap with the updated order and return it
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public SalesOrder Refund(string orderId, string warehouse, string by)
    {
        // get the existed sale order and ensure it is delivered
        var existedOrder = FindById(orderId);
        if(existedOrder.Status != SaleStatus.Delivered)
            throw new InvalidOrderStateException($"Can only refund Delivered orders. Current Status: {existedOrder.Status}");

        // return stock at original cost
        foreach (var item in existedOrder.Items.Where(l => l.QtyShipped > 0))
            _ledger.StockIn(
                productId: item.ProductId,
                quantity: item.QtyShipped,
                unitCost: item.UnitCost,
                warehouse: warehouse,
                reference: $"REFUND:{orderId}",
                by: by
            );

        // mark the sale order as refunded
        var updatedOrder = existedOrder with { Status = SaleStatus.Refunded };

        // update the ordersMap with the updated order and return it
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public SalesOrder FindById(string orderId) =>
        _ordersMap.TryGetValue(orderId, out var existedOrder)
            ? existedOrder
            : throw new OrderNotFoundException(orderId);

    public IReadOnlyList<SalesOrder> FindByStatus(SaleStatus status) => [..
        from order in _ordersMap.Values
        where order.Status == status
        orderby order.CreatedAt descending
        select order
    ];

}
