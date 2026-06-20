namespace InventoryManagement;

public record ProductProfit
(
    string  Sku,
    string  Name,
    int     UnitsSold,
    decimal Revenue,
    decimal Cost,
    decimal GrossProfit,
    decimal ProfitMargin
)
{
    public override string ToString() =>
        $"[Sku] - [Name] -> ({UnitsSold}) sold | ({Revenue}) revenue |" +
        $"({Cost}) cost | ({GrossProfit}) GP | ({ProfitMargin}) margin";
}
