// ═══════════════════════════════════════════════════════════════════════════════════════
//  INVENTORY SYSTEM — facade composing all 7 components
// ═══════════════════════════════════════════════════════════════════════════════════════

using System.Text;
using System.Text.Json.Serialization;
using System.Collections.Frozen;

namespace InventoryManagement;

public sealed class Inventory
{
    private readonly string _dataDirectory = Path.Combine(Helpers.SameDirectory(), "data");
    private readonly string _name;
    private readonly IProductCatalog _catalog;
    private readonly ISupplierRegistry _suppliers;
    private readonly ICustomerRegistry _customers;
    private readonly IStockLedger _ledger;
    private readonly IPurchaseManager  _purchases;
    private readonly ISalesManager _sales;
    private readonly IProfitAnalyzer _analyzer;

    public string Name => _name;
    [JsonIgnore]
    public string FileName => Path.Combine(_dataDirectory, $"{_name.Replace(" ", "_").ToLower()}.json");

    public Inventory(string name)
    {
        _name = name;
        _suppliers  = new SupplierRegistry();
        _catalog    = new ProductCatalog();
        _customers  = new CustomerRegistry();
        _ledger     = new StockLedger();
        _purchases  = new PurchaseManager(_ledger);
        _sales      = new SalesManager(_ledger);
        _analyzer   = new ProfitAnalyzer();
    }

    // ── products facade ──────────────────────────────────────────────────────────────────
    public Product AddNewProduct(
        string sku,
        string name,
        string description,
        decimal listPrice,
        ProductCategory category,
        string unit = "pcs"
    ) => _catalog.Add(
        sku: sku,
        name: name,
        description: description,
        listPrice: listPrice,
        category: category,
        unit: unit
    );
    public IEnumerable<Product> Products => _catalog.All;
    public Product FindProduct(string productId) => _catalog.FindById(productId);

    // ── suppliers facade ──────────────────────────────────────────────────────────────────
    public Supplier AddNewSupplier(
        string name,
        string email,
        string mobile,
        string terms
    ) => _suppliers.Add(
        name: name,
        email: email,
        mobile: mobile,
        terms: terms
    );
    public IEnumerable<Supplier> Suppliers => _suppliers.All;
    public Supplier FindSupplier(string supplierId) => _suppliers.Find(supplierId);

    // ── customers facade ──────────────────────────────────────────────────────────────────
    public Customer AddNewCustomer(
        string name,
        string email,
        string mobile,
        CustomerTier tier = CustomerTier.Standard,
        decimal discountRate = 0
    ) => _customers.Add(
        name: name,
        email: email,
        mobile: mobile,
        tier: tier,
        discountRate: discountRate
    );
    public IEnumerable<Customer> Customers => _customers.All;
    public Customer FindCustomer(string customerId) => _customers.Find(customerId);

    // ── stock queries ────────────────────────────────────────────────────────────────────
    public int StockLevel(string productId, string? warehouse = null)
        => _ledger.ProductLevel(productId, warehouse);
    public decimal AverageCost(string pid)
        => _ledger.AvgCost(pid);
    public IReadOnlyList<(string Pid, int Qty)> LowStockReport(int threshold = 10)
        => _ledger.LowStock(threshold);
    public IReadOnlyList<StockMovement> StockMovements => _ledger.Movements;
    public FrozenDictionary<string, Dictionary<string, List<CostLayer>>> CostsMap => _ledger.CostsMap;

    // ── purchase flow: Create → Receive → Cancel → Pay ───────────────────────────────────
    public IReadOnlyList<PurchaseOrder> PurchasesOrders => _purchases.All;
    public PurchaseOrder CreatePurchaseOrder(
        string supplierId,
        IEnumerable<(string ProductId, int Quantity, decimal UnitCost)> items
    )
    {
        var existedSupplier = _suppliers.Find(supplierId);
        var orderItems = items.Select(item =>
        {
            var existedProduct = _catalog.FindById(item.ProductId);
            return (
                existedProduct.ProductId,
                existedProduct.Name,
                existedProduct.Sku,
                item.Quantity,
                item.UnitCost
            );
        });
        return _purchases.Create(supplierId, existedSupplier.Name, orderItems);
    }
    public PurchaseOrder ReceivePurchaseOrder(
        string orderId,
        IEnumerable<(string ProductId, int Qty)> receipts,
        string warehouse = "WAREHOUSE-MAIN",
        string by = "system"
    ) => _purchases.Receive(
        orderId: orderId,
        receipts: receipts,
        warehouse: warehouse,
        by: by
    );

    public PurchaseOrder CancelPurchaseOrder(string orderId) => _purchases.Cancel(orderId);
    public PurchaseOrder PayPurchaseOrder(string orderId) => _purchases.MarkPaid(orderId);

    // ── sales flow: Create → Cancel → Ship → Deliver → Pay → Return ───────────────────────
    public IReadOnlyList<SalesOrder> SalesOrders => _sales.All;

    public SalesOrder CreateSaleOrder(
        string customerId,
        IEnumerable<(string ProductId, int Quantity, decimal UnitPrice)> items
    )
    {
        var existedCustomer = _customers.Find(customerId);
        var orderItems = items.Select(item =>
        {
            var existedProduct = _catalog.FindById(item.ProductId);
            var availableStock = _ledger.ProductLevel(existedProduct.ProductId);
            if (item.Quantity > availableStock)
                throw new InsufficientStockException(
                    existedProduct.ProductId,
                    availableStock,
                    item.Quantity
                );
            return (
                existedProduct.ProductId,
                existedProduct.Name,
                existedProduct.Sku,
                item.Quantity,
                item.UnitPrice
            );
        });
        return _sales.Create(
            customerId: existedCustomer.CustomerId,
            customerName: existedCustomer.Name,
            discountRate: existedCustomer.DiscountRate,
            items: orderItems
        );
    }
    public SalesOrder CancelSaleOrder(string orderId) => _sales.Cancel(orderId);
    public SalesOrder ShipSaleOrder(
        string orderId,
        string warehouse = "WAREHOUSE-MAIN",
        string by = "system"
    ) => _sales.Ship(
        orderId: orderId,
        warehouse: warehouse,
        by: by
    );
    public SalesOrder DeliverSaleOrder(string orderId) => _sales.Deliver(orderId);
    public SalesOrder PaySaleOrder(string orderId) => _sales.MarkPaid(orderId);
    public SalesOrder ReturnSaleOrder(
        string orderId,
        string warehouse = "WAREHOUSE-MAIN",
        string by = "system"
    ) => _sales.Refund(
        orderId: orderId,
        warehouse: warehouse,
        by: by
    );

    // ── profit report ───────────────────────────────────────────────────────────────────
    public ProfitReport ProfitReport(
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        var start = startDate ?? DateTime.MinValue;
        var end = endDate ?? DateTime.MaxValue;
        return _analyzer.GenerateReport(
            startDate: start,
            endDate: end,
            purchases: _purchases.All,
            sales: _sales.All,
            movements: _ledger.Movements
        );
    }

    // ── formatted profit report string ───────────────────────────────────────────────────
    public string ProfitReportString(
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        var report = ProfitReport(startDate, endDate);
        var sBuilder  = new StringBuilder();

        string divider  = new('═', 80);
        string line = new('─', 80);

        sBuilder.AppendLine(divider);
        sBuilder.AppendLine($"  {Name.ToUpper()} — PROFIT & LOSS REPORT");
        sBuilder.AppendLine($"  Period: {report.From:dd MMM yyyy} — {report.To:dd MMM yyyy}");
        sBuilder.AppendLine(divider);
        sBuilder.AppendLine($"  {"Orders delivered",-36} {report.TotalOrders,8}");
        sBuilder.AppendLine($"  {"Units sold",-36} {report.TotalUnitsSold,8}");
        sBuilder.AppendLine();
        sBuilder.AppendLine($"  {"Gross Revenue",-36} {report.GrossRevenue,12:C}");
        sBuilder.AppendLine($"  {"Cost of Goods Sold (COGS)",-36} {report.TotalCost,12:C}");
        sBuilder.AppendLine($"  {line}");
        sBuilder.AppendLine($"  {"Gross Profit",-36} {report.GrossProfit,12:C}");
        sBuilder.AppendLine($"  {"Gross Margin",-36} {report.GrossProfitMargin,12:P1}");
        sBuilder.AppendLine();
        sBuilder.AppendLine($"  {"Net Profit",-36} {report.NetProfit,12:C}");
        sBuilder.AppendLine(divider);
        sBuilder.AppendLine();

        sBuilder.AppendLine(
            $"  {"SKU",-20} {"Product",-20} {"Sold",-4} " +
            $"{"Revenue",10} {"COGS",10} {"GP",10} {"Margin",-5}"
        );
        sBuilder.AppendLine($"  {line}");

        foreach (var p in report.ProductsProfits)
        {
            string flag = p.ProfitMargin < 0.15m ? " ⚠" : "";
            sBuilder.AppendLine(
                $"  {p.Sku,-20} {p.Name,-20} {p.UnitsSold,-4} | " +
                $"{p.Revenue,10:C} {p.Cost,10:C} {p.GrossProfit,10:C} | " +
                $"{p.ProfitMargin,5:P1}{flag}"
            );
        }
        sBuilder.AppendLine(divider);

        return sBuilder.ToString();
    }

    // ── inventory snapshot ──────────────────────────────────────────────────────────────
    [JsonIgnore]
    public string InventorySnapshot
    {
        get
        {
            var sBuilder  = new StringBuilder();
            string divider  = new('═', 80);
            string line = new('─', 80);

            sBuilder.AppendLine(divider);
            sBuilder.AppendLine($"  {Name.ToUpper()} — INVENTORY SNAPSHOT");
            sBuilder.AppendLine($"  {DateTime.Now:dd MMM yyyy HH:mm}");
            sBuilder.AppendLine(divider);

            sBuilder.AppendLine(
                $"  {"SKU",-18} {"Product",-30} {"Category",-14} " +
                $"{"List Price":>10} {"AvgCost":>10} {"Stock":>8} {"Value":>10}"
            );
            sBuilder.AppendLine($"  {line}");

            decimal totalCost = 0;
            foreach (var product in _catalog.All)
            {
                int quantity = _ledger.ProductLevel(product.ProductId);
                decimal cost = _ledger.AvgCost(product.ProductId);
                decimal subTotal = quantity * cost;
                totalCost += subTotal;

                string flag = quantity == 0 ? " OUT" : quantity < 10 ? " LOW" : "";
                sBuilder.AppendLine(
                    $"  {product.Sku,-18} {product.Name,-30} {product.Category,-14} " +
                    $"{product.ListPrice,10:C} {cost,10:C} {quantity,8}  {subTotal,10:C}{flag}"
                );
            }

            sBuilder.AppendLine($"  {line}");
            sBuilder.AppendLine($"  {"Total inventory value",-64} {totalCost,10:C}");
            sBuilder.AppendLine(divider);

            return sBuilder.ToString();
        }
    }

    // ── inventory costs layers ──────────────────────────────────────────────────────────
    [JsonIgnore]
    public string InventoryCostsMapString
    {
        get
        {
            var sBuilder  = new StringBuilder();
            string divider  = new('═', 80);
            string line = new('─', 80);

            sBuilder.AppendLine(divider);
            sBuilder.AppendLine($"  {Name.ToUpper()} — INVENTORY COSTS LAYERS");
            sBuilder.AppendLine($"  {DateTime.Now:dd MMM yyyy HH:mm}");
            sBuilder.AppendLine(divider);

            sBuilder.AppendLine(
                $"  {"SKU",-18} {"Product",-30} {"Warehouse",-14} " +
                $"{"Cost":>14} {"Quantity":>10} {"SubTotal":>10}"
            );
            sBuilder.AppendLine($"  {line}");

            Product _product;
            foreach (var (productId, warehousesMap) in _ledger.CostsMap)
            {
                _product = _catalog.FindById(productId);
                foreach (var (warehouse, costs) in warehousesMap)
                {
                    foreach (var cost in costs)
                    {
                        decimal subTotal = cost.QuantityRemaining * cost.UnitCost;
                        sBuilder.AppendLine(
                            $"  {_product.Sku,-18} {productId,-30} {warehouse,-14} " +
                            $"{cost.UnitCost,14:C} {cost.QuantityRemaining,10}  {subTotal,10:C}"
                        );
                    }
                }
            }
            sBuilder.AppendLine($"  {line}");
            sBuilder.AppendLine(divider);

            return sBuilder.ToString();
        }
    }


    // ── write data to JSON file ─────────────────────────────────────────────────────────
    public async Task SaveToJsonFileAsync()
    {
        if(!Directory.Exists(_dataDirectory))
            Directory.CreateDirectory(_dataDirectory);
        await Helpers.WriteToJsonFileAsync(
            filePath: Path.Combine(_dataDirectory, $"{FileName}"),
            data: this
        );
    }

}
