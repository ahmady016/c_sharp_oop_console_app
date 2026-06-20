namespace InventoryManagement;

public interface IPurchaseManager
{
    IReadOnlyList<PurchaseOrder> All { get; }
    PurchaseOrder Create (
        string supplierId,
        string supplierName,
        IEnumerable<(string ProductId, string Name, string Sku, int Quantity, decimal UnitCost)> items
    );
    PurchaseOrder Receive(
        string orderId,
        IEnumerable<(string ProductId, int Quantity)> receipts,
        string warehouse,
        string by
    );
    PurchaseOrder MarkPaid(string orderId);
    PurchaseOrder Cancel(string orderId);
    PurchaseOrder FindById(string orderId);
    IReadOnlyList<PurchaseOrder> FindByStatus(PurchaseStatus status);
}

public sealed class PurchaseManager : IPurchaseManager
{
    private int _seq = 0;
    private readonly IStockLedger _ledger;
    private readonly Dictionary<string, PurchaseOrder> _ordersMap = [];
    public IReadOnlyList<PurchaseOrder> All => [..
        from order in _ordersMap.Values
        orderby order.CreatedAt descending
        select order
    ];

    public PurchaseManager(IStockLedger ledger) => _ledger = ledger;

    public PurchaseOrder Create(
        string supplierId,
        string supplierName,
        IEnumerable<(string ProductId, string Name, string Sku, int Quantity, decimal UnitCost)> items
    )
    {
        var orderItems = items
            .Select(item => new PurchaseItem(
                ProductId: item.ProductId,
                ProductName: item.Name,
                Sku: item.Sku,
                QtyOrdered: item.Quantity,
                QtyReceived: 0,
                UnitCost: item.UnitCost
            ))
            .ToList();
        var newPurchaseOrder = new PurchaseOrder(
            OrderId:       $"PURCHASE-{++_seq:D8}",
            SupplierId:    supplierId,
            SupplierName:  supplierName,
            Items:         orderItems,
            Status:        PurchaseStatus.Submitted,
            PaymentStatus: PaymentStatus.Unpaid,
            CreatedAt:     DateTime.Now,
            ReceivedAt:    null
        );
        _ordersMap[newPurchaseOrder.OrderId] = newPurchaseOrder;
        return newPurchaseOrder;
    }

    public PurchaseOrder FindById(string orderId) =>
        _ordersMap.TryGetValue(orderId, out var existedOrder)
            ? existedOrder
            : throw new OrderNotFoundException(orderId);

    public IReadOnlyList<PurchaseOrder> FindByStatus(PurchaseStatus status) => [..
        from order in _ordersMap.Values
        where order.Status == status
        orderby order.CreatedAt descending
        select order
    ];

    public PurchaseOrder Receive(
        string orderId, IEnumerable<(string ProductId, int Quantity)> receipts,
        string warehouse,
        string by
    )
    {
        // get existing order and ensure it's not fully received or cancelled
        var existedOrder = FindById(orderId);
        if (existedOrder.Status == PurchaseStatus.Cancelled)
            throw new InvalidOrderStateException($"PO {orderId} is cancelled.");
        if (existedOrder.Status == PurchaseStatus.FullyReceived)
            throw new InvalidOrderStateException($"PO {orderId} already fully received.");

        // update each order item received quantity
        var receiptsMap = receipts.ToDictionary(r => r.ProductId, r => r.Quantity);
        var updatedItems = existedOrder.Items
            .Select(item => {
                if(!receiptsMap.TryGetValue(item.ProductId, out int receivedQuantity))
                    return item;
                int actual = Math.Min(receivedQuantity, item.Pending);
                _ledger.StockIn(item.ProductId, actual, item.UnitCost, warehouse, existedOrder.OrderId, by);
                return item with { QtyReceived = item.QtyReceived + actual };
            })
            .ToList();

        // if all items are received, mark the order as fully received
        // otherwise mark it as partially received
        bool full = updatedItems.All(l => l.Pending == 0);
        var status = full
            ? PurchaseStatus.FullyReceived
            : PurchaseStatus.PartialReceived;

        // update the order with updated items and status
        // and update the receivedAt datetime if the order is fully received
        var updatedOrder = existedOrder with
        {
            Items      = updatedItems,
            Status     = status,
            ReceivedAt = full ? DateTime.Now : existedOrder.ReceivedAt
        };
        // finally update the order in orders map and return the updated order
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public PurchaseOrder MarkPaid(string orderId)
    {
        var existedOrder = FindById(orderId);
        var updatedOrder = existedOrder with { PaymentStatus = PaymentStatus.Paid };
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

    public PurchaseOrder Cancel(string orderId)
    {
        var existedOrder = FindById(orderId);
        if (existedOrder.Status == PurchaseStatus.FullyReceived)
            throw new InvalidOrderStateException("Cannot cancel a fully received Purchase Order.");
        var updatedOrder = existedOrder with { Status = PurchaseStatus.Cancelled };
        _ordersMap[orderId] = updatedOrder;
        return updatedOrder;
    }

}
