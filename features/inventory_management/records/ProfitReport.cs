namespace InventoryManagement;

public record ProfitReport
(
    DateTime From,
    DateTime To,
    decimal TotalCost,
    decimal GrossRevenue,
    decimal GrossProfit,
    decimal GrossProfitMargin,
    decimal OperatingExpenses,
    decimal NetProfit,
    int TotalUnitsSold,
    int TotalOrders,
    IReadOnlyList<ProductProfit> ProductsProfits
);
