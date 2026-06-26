namespace InventoryManagement;

public interface IProfitAnalyzer
{
    ProfitReport GenerateReport(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<PurchaseOrder> purchases,
        IReadOnlyList<SalesOrder> sales,
        IReadOnlyList<StockMovement> movements
    );
}

public sealed class ProfitAnalyzer : IProfitAnalyzer
{
    public ProfitReport GenerateReport(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<PurchaseOrder> purchases,
        IReadOnlyList<SalesOrder> sales,
        IReadOnlyList<StockMovement> movements
    )
    {
        var deliveredSales = sales
            .Where(s =>
                s.CreatedAt >= startDate && s.CreatedAt <= endDate &&
                s.Status is SaleStatus.Delivered or SaleStatus.Shipped
            ).ToList();

        decimal totalCost = deliveredSales.Sum(s => s.TotalCost);
        decimal grossRevenue = deliveredSales.Sum(s => s.GrossRevenue);
        decimal grossProfit = grossRevenue - totalCost;
        decimal grossMargin = grossRevenue == 0 ? 0 : grossProfit / grossRevenue;

        // total purchases paid in period = operating cost
        decimal operatingExpenses = purchases
            .Where(p =>
                p.CreatedAt >= startDate &&
                p.CreatedAt <= endDate &&
                p.Status != PurchaseStatus.Cancelled
            ).Sum(p => p.ReceivedCost);

        // simplified: GrossProfit as Net (no overhead model)
        decimal netProfit = grossProfit;
        int unitsSold = (
            from sale in deliveredSales
            from item in sale.Items
            where item.QtyShipped > 0
            select item.QtyShipped
        ).Sum();
        int ordersCount = deliveredSales.Count;

        // per-product breakdown
        var perProduct =
            from sale in deliveredSales
            from item in sale.Items
            where item.QtyShipped > 0
            group item by new ProductFullName(item.ProductId, item.Sku, item.ProductName) into itemGroup
            let revenue = itemGroup.Sum(item => item.Revenue)
            let cost = itemGroup.Sum(item => item.Cost)
            let gProfit = revenue - cost
            orderby gProfit descending
            select new ProductProfit(
                Sku: itemGroup.Key.Sku,
                Name: itemGroup.Key.Name,
                UnitsSold: itemGroup.Sum(item => item.QtyShipped),
                Revenue: revenue,
                Cost: cost,
                GrossProfit: gProfit,
                ProfitMargin: revenue == 0 ? 0 : gProfit / revenue
            );

        // finally return the Profit Report
        return new ProfitReport(
            From: startDate,
            To: endDate,
            GrossRevenue: grossRevenue,
            TotalCost: totalCost,
            GrossProfit: grossProfit,
            GrossProfitMargin: grossMargin,
            OperatingExpenses: operatingExpenses,
            NetProfit: netProfit,
            TotalUnitsSold: unitsSold,
            TotalOrders: ordersCount,
            ProductsProfits: [..perProduct]
        );
    }

}
