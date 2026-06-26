using System.Collections.Frozen;

namespace InventoryManagement;

public interface IStockLedger
{
    FrozenDictionary<string, Dictionary<string, List<CostLayer>>> CostsMap { get; }
    public IReadOnlyList<StockMovement> Movements { get; }
    int ProductLevel(string productId, string? warehouse = null);
    decimal StockIn(string productId, int quantity, decimal unitCost, string warehouse, string reference, string by);
    decimal StockOut(string productId, int quantity, string warehouse, string reference, string by);
    decimal AvgCost(string productId);
    void Adjust(string productId, int delta, string warehouse, string note, string by);
    IReadOnlyList<StockMovement> History(string productId);
    IReadOnlyList<(string ProductId, int Quantity)> LowStock(int threshold);
}

public sealed class StockLedger : IStockLedger
{
    private int _seq = 0;
    // productId → warehousesMap (warehouse → List<CostLayer>) (FIFO queue per warehouse)
    private readonly Dictionary<string, Dictionary<string, List<CostLayer>>> _costsMap = [];
    private readonly List<StockMovement> _movements = [];
    public FrozenDictionary<string, Dictionary<string, List<CostLayer>>> CostsMap => _costsMap.ToFrozenDictionary();
    public IReadOnlyList<StockMovement> Movements => _movements.AsReadOnly();

    private void InitializeCostBuckets(string productId, string warehouse)
    {
        if (!_costsMap.ContainsKey(productId))
            _costsMap[productId] = [];
        if (!_costsMap[productId].ContainsKey(warehouse))
            _costsMap[productId][warehouse] = [];
    }

    private void RecordMovement(
        string productId,
        MovementType type,
        int quantity,
        decimal cost,
        decimal price,
        string warehouse,
        string reference,
        string by
    )
    {
        _movements.Add(new StockMovement(
            MovementId: $"MOV-{++_seq:D9}",
            ProductId: productId,
            Type: type,
            Quantity: quantity,
            UnitCost: cost,
            UnitPrice: price,
            Warehouse: warehouse,
            Reference: reference,
            Timestamp: DateTime.Now,
            PerformedBy: by
        ));
    }

    public int ProductLevel(string productId, string? warehouse = null)
    {
        // case 1: return 0 if no such product in stock
        if (!_costsMap.TryGetValue(productId, out var warehousesMap))
            return 0;
        // case 2: return total quantity in this warehouse, 0 if no such warehouse
        if (warehouse is not null)
            return warehousesMap.TryGetValue(warehouse, out var costLayers)
                ? costLayers.Sum(l => l.QuantityRemaining)
                : 0;
        // case 3: return total quantities from all warehouses in stock
        return warehousesMap.Values
            .SelectMany(l => l)
            .Sum(l => l.QuantityRemaining);
    }

    public decimal StockIn(
        string productId, int quantity, decimal unitCost,
        string warehouse, string reference, string by
    )
    {
        InitializeCostBuckets(productId, warehouse);
        _costsMap[productId][warehouse].Add(
            new CostLayer(reference, quantity, unitCost, DateTime.Now)
        );
        RecordMovement(
            productId: productId,
            type: MovementType.PurchaseIn,
            quantity: quantity,
            cost: unitCost,
            price: 0m,
            warehouse: warehouse,
            reference: reference,
            by: by
        );
        return unitCost;
    }

    public decimal StockOut(
        string productId, int quantity, string warehouse, string reference, string by
    )
    {
        int available = ProductLevel(productId, warehouse);
        if (quantity > available)
            throw new InsufficientStockException(productId, available, quantity);

        // FIFO calculation: consume products (costs layers) from oldest to newest
        int remaining = quantity;
        decimal totalCost = 0m;
        // get FIFO costs queue of this product in this warehouse
        var costs = _costsMap[productId][warehouse];
        for(int i = 0; i < costs.Count; i++)
        {
            if(remaining <= 0) break;
            var cost = costs[i];
            int taken = Math.Min(remaining, cost.QuantityRemaining);
            totalCost += taken * cost.UnitCost;
            remaining -= taken;
            costs[i] = cost with
            {
                QuantityRemaining = cost.QuantityRemaining - taken
            };
        }
        // remove exhausted layers (quantities taken before)
        costs.RemoveAll(l => l.QuantityRemaining == 0);

        // record movement
        decimal avgCost = totalCost / quantity;
        RecordMovement(
            productId: productId,
            type: MovementType.SaleOut,
            quantity: quantity,
            cost: avgCost,
            price: 0m,
            warehouse: warehouse,
            reference: reference,
            by: by
        );
        // FIFO calculated unit cost returned to caller
        return avgCost;
    }

    public decimal AvgCost(string productId)
    {
        // case 1: return 0 if no such product in stock
        if (!_costsMap.TryGetValue(productId, out var warehousesMap))
            return 0m;
        // case 2: get all warehouses with remaining quantity, return 0 if no remaining
        var all = warehousesMap.Values
            .SelectMany(l => l)
            .Where(l => l.QuantityRemaining > 0)
            .ToList();
        if (all.Count == 0)
            return 0m;
        // case 3: return average cost from all warehouses
        decimal totalCost = all.Sum(l => l.QuantityRemaining * l.UnitCost);
        int totalQuantity = all.Sum(l => l.QuantityRemaining);
        return totalQuantity == 0 ? 0m : totalCost / totalQuantity;
    }

    public void Adjust(
        string productId, int delta, string warehouse, string note, string by
    )
    {
        InitializeCostBuckets(productId, warehouse);
        decimal productCost = AvgCost(productId);
        // case 1: addition of quantity with current average cost (same as stockIn)
        if(delta > 0)
            _costsMap[productId][warehouse].Add(
                new CostLayer("ADJUST", delta, productCost, DateTime.Now)
            );
        // case 2: subtraction of quantity (same as stockOut)
        else
        {
            int quantity = Math.Abs(delta);
            int available = ProductLevel(productId, warehouse);
            if (quantity > available)
                throw new InsufficientStockException(productId, available, quantity);

            var costs = _costsMap[productId][warehouse];
            int remaining = quantity;
            for (int i = 0; i < costs.Count; i++)
            {
                if(remaining <= 0) break;
                var cost = costs[i];
                int take = Math.Min(remaining, cost.QuantityRemaining);
                remaining -= take;
                costs[i] = cost with
                {
                    QuantityRemaining = cost.QuantityRemaining - take
                };
            }
            costs.RemoveAll(l => l.QuantityRemaining == 0);
        }
        RecordMovement(
            productId: productId,
            type: MovementType.Adjustment,
            quantity: Math.Abs(delta),
            cost: productCost,
            price: 0m,
            warehouse: warehouse,
            reference: note,
            by: by
        );
    }

    public IReadOnlyList<StockMovement> History(string productId) => [..
        from movement in _movements
        where movement.ProductId == productId
        orderby movement.Timestamp
        select movement
    ];

    public IReadOnlyList<(string ProductId, int Quantity)> LowStock(int threshold) => [..
        from productId in _costsMap.Keys
        let quantity = ProductLevel(productId)
        where quantity <= threshold
        orderby quantity
        select (productId, quantity)
    ];

}
