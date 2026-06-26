// ╔══════════════════════════════════════════════════════════════════════════╗
// ║   INVENTORY MANAGEMENT SYSTEM — Full Purchase + Sales Cycle                ║
// ║   Net Profit Analytics using FIFO Cost Tracking                            ║
// ║                                                                            ║
// ║   Components:                                                              ║
// ║     ProductCatalog    — SKU registry                                       ║
// ║     StockLedger       — stock movements + FIFO cost layers                 ║
// ║     SupplierRegistry  — supplier management                                ║
// ║     PurchaseManager   — purchase orders → stock in                         ║
// ║     SalesManager      — sales orders → stock out + revenue                 ║
// ║     ProfitAnalyzer    — COGS / gross profit / net profit reports           ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using Bogus;

namespace InventoryManagement;

public static class InventoryManager
{
    private static readonly Faker _faker = new();
    private static readonly Inventory _rayaTechStore = new("RayaTechStore");
    private static readonly Dictionary<string, Product?> _products = [];
    private static readonly Dictionary<string, Supplier?> _suppliers = [];
    private static readonly Dictionary<string, Customer?> _customers = [];
    private static readonly Dictionary<string, SalesOrder?> _salesOrders = [];

    private static T? Add<T>(string message, Func<T> action) where T : class
    {
        Console.Write($"  {message,-50}");
        try
        {
            var r = action();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" ✓");
            Console.ResetColor();
            return r;
        }
        catch (InventoryException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" ✗  {ex.Message}");
            Console.ResetColor();
            return null;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" ✗  {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
    private static void Do(string message, Action action)
    {
        Console.Write($"  {message,-50}");
        try
        {
            action();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" ✓");
        }
        catch (InventoryException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" ✗  {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" ✗  {ex.Message}");
        }
        finally { Console.ResetColor(); }
    }

    private static void GenerateProducts()
    {
        _products["laptopPro15"] = Add(
            "Adding Laptop Pro 15",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-LP15", "Laptop Pro 15", "15\" 16GB 512GB", 1_400m, ProductCategory.Electronics)
        );
        _products["wirelessMouse"] = Add(
            "Adding Wireless Mouse",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-WM01", "Wireless Mouse", "Ergonomic 2.4G", 49.99m, ProductCategory.Electronics)
        );
        _products["mechKeyboard"] = Add(
            "Adding Mech Keyboard",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-MK02", "Mech Keyboard", "TKL Blue switches", 129.99m, ProductCategory.Electronics)
        );
        _products["monitor27"] = Add(
            "Adding 27\" Monitor",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-MN27", "27-inch Monitor", "4K 144Hz IPS", 599.99m, ProductCategory.Electronics)
        );
        _products["ergonomicChair"] = Add(
            "Adding Ergonomic Chair",
            () => _rayaTechStore.AddNewProduct("FURNITURE-CH01", "Ergonomic Chair", "Lumbar adj", 349.99m, ProductCategory.Furniture)
        );
        _products["ryzen5"] = Add(
            "Adding Ryzen CPU",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-CP01", "Ryzen CPU", "Ryzen 5 3600", 199.99m, ProductCategory.Electronics)
        );
        _products["ryzen7"] = Add(
            "Adding Ryzen CPU",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-CP02", "Ryzen CPU", "Ryzen 7 3700", 299.99m, ProductCategory.Electronics)
        );
        _products["ryzen9"] = Add(
            "Adding Ryzen CPU",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-CP03", "Ryzen CPU", "Ryzen 9 3900", 399.99m, ProductCategory.Electronics)
        );
        _products["dd04Ram"] = Add(
            "Adding 4GB RAM",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-RM01", "kingston Fury Beast 4GB RAM", "DDR4 2666MHz", 99.99m, ProductCategory.Electronics)
        );
        _products["dd08Ram"] = Add(
            "Adding 8GB RAM",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-RM02", "kingston Fury Beast 8GB RAM", "DDR4 2666MHz", 199.99m, ProductCategory.Electronics)
        );
        _products["dd16Ram"] = Add(
            "Adding 16GB RAM",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-RM03", "kingston Fury Beast 16GB RAM", "DDR4 2666MHz", 399.99m, ProductCategory.Electronics)
        );
        _products["amdRadeonRX6600"] = Add(
            "Adding Radeon RX6600",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-GP01", "Radeon RX6600", "8GB GDDR6", 799.99m, ProductCategory.Electronics)
        );
        _products["amdRadeonRX6700"] = Add(
            "Adding Radeon RX6800",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-GP02", "Radeon RX6800", "8GB GDDR6", 999.99m, ProductCategory.Electronics)
        );
        _products["amdRadeonRX7600"] = Add(
            "Adding Radeon RX6900",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-GP03", "Radeon RX6900", "8GB GDDR6", 1199.99m, ProductCategory.Electronics)
        );
        _products["geForceRTX2060"] = Add(
            "Adding RTX2060",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-GP04", "RTX2060", "8GB GDDR6", 799.99m, ProductCategory.Electronics)
        );
        _products["geForceRTX2070"] = Add(
            "Adding RTX2070",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-GP05", "RTX2070", "8GB GDDR6", 999.99m, ProductCategory.Electronics)
        );
        _products["geForceRTX2080"] = Add(
            "Adding RTX2080",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-GP06", "RTX2080", "8GB GDDR6", 1199.99m, ProductCategory.Electronics)
        );
        _products["samsungSSD980Pro"] = Add(
            "Adding Samsung SSD980 Pro",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-SS01", "Samsung SSD980 Pro", "2TB", 499.99m, ProductCategory.Electronics)
        );
        _products["kingstonSSD"] = Add(
            "Adding Kingston SSD",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-SS02", "Kingston SSD", "2TB", 399.99m, ProductCategory.Electronics)
        );
        _products["lexarSSD"] = Add(
            "Adding Lexar SSD",
            () => _rayaTechStore.AddNewProduct("ELECTRONICS-SS03", "Lexar SSD", "2TB", 499.99m, ProductCategory.Electronics)
        );
    }

    private static Customer GenerateRandomCustomer() => _rayaTechStore
        .AddNewCustomer(
            name: _faker.Name.FullName(),
            email: _faker.Internet.Email(),
            mobile: _faker.Phone.PhoneNumber("01#########"),
            tier: _faker.Random.Enum<CustomerTier>(),
            discountRate: _faker.Random.Decimal(0, 0.25m)
        );
    private static void GenerateCustomers(int count = 70)
    {
        Customer? customer;
        for (int i = 0; i < count; i++)
        {
            customer = Add($"Adding Customer #{i+1:00}", GenerateRandomCustomer);
            _customers[customer!.FullName] = customer;
        }
    }

    private static Supplier GenerateRandomSupplier() => _rayaTechStore
        .AddNewSupplier(
            name: _faker.Company.CompanyName(),
            email: _faker.Internet.Email(),
            mobile: _faker.Phone.PhoneNumber("01#########"),
            terms: _faker.Random.Enum<PaymentTerm>().ToString().ToLower()
        );
    private static void GenerateSuppliers(int count = 10)
    {
        Supplier? supplier;
        for (int i = 0; i < count; i++)
        {
            supplier = Add($"Adding Supplier #{i+1:00}", GenerateRandomSupplier);
            _suppliers[supplier!.Name] = supplier;
        }
    }

    private static PurchaseOrder AllProductPurchaseOrder() => _rayaTechStore
        .CreatePurchaseOrder(
            supplierId: Helpers.PickOne(_suppliers.Values)!.SupplierId,
            items: _products.Values.Select(product => (
                product!.ProductId,
                _faker.Random.Int(10, 120),
                product!.ListPrice
            )
        ));
    private static void ExecuteAllProductPurchaseCycle()
    {
        var allProductOrder = AllProductPurchaseOrder();
        ReceivePurchaseOrder(allProductOrder);
        PayPurchaseOrder(allProductOrder);
        Console.WriteLine($"All Product Purchase Order -> {allProductOrder}");
    }
    private static PurchaseOrder GetRandomPurchaseOrder() => _rayaTechStore
        .CreatePurchaseOrder(
            supplierId: Helpers.PickOne(_suppliers.Values)!.SupplierId,
            items: Helpers
                .PickSet(collection: _products.Values, count: _faker.Random.Int(3, 7))
                .Select(product => (
                    product!.ProductId,
                    _faker.Random.Int(10, 80),
                    product!.ListPrice
                )
        ));
    private static void ReceivePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        Do($"Fully Receive Purchase Order -> ({purchaseOrder.OrderId})", () =>
        {
            var receiptOrder = _rayaTechStore.ReceivePurchaseOrder(
                orderId: purchaseOrder.OrderId,
                receipts: purchaseOrder.Items.Select(item => (item.ProductId, item.QtyOrdered)),
                warehouse: "WAREHOUSE-MAIN",
                by: "SYSTEM"
            );
            Console.WriteLine($"  {receiptOrder}");
        });
    }
    private static void PayPurchaseOrder(PurchaseOrder purchaseOrder)
    {
        Do(
            $"Mark Paid Purchase Order -> ({purchaseOrder.OrderId})",
            () => _rayaTechStore.PayPurchaseOrder(purchaseOrder.OrderId)
        );
    }
    private static void ExecuteRandomPurchasesCycles(int count = 10)
    {
        PurchaseOrder? purchaseOrder;
        for (int i = 0; i < count; i++)
        {
            purchaseOrder = Add($"Creating Random Purchase Order #{i+1:00}", GetRandomPurchaseOrder);
            Console.WriteLine(purchaseOrder);
            ReceivePurchaseOrder(purchaseOrder!);
            if(Helpers.PickOne([true, false, true, true, false, true, false, true, true]))
                PayPurchaseOrder(purchaseOrder!);
        }
    }

    private static bool ExecuteSaleCycleByGoldCustomer()
    {
        var goldCustomer = _customers.Values.FirstOrDefault(c => c!.Tier == CustomerTier.Gold);
        if(goldCustomer is null)
        {
            Helpers.PrintWarning("Opps, No Gold Customer Found");
            return false;
        }

        // SaleOrder01: Gold Customer buys 2 laptops + 3 mice (Gold 10% discount)
        var saleOrder01 = Add(
            $"SaleOrder01: {goldCustomer.Name} buys 2×Laptop + 3×Mouse",
            () => {
                var laptopPro15 = _products["laptopPro15"]!;
                decimal laptopPro15Price = laptopPro15.ListPrice + (laptopPro15.ListPrice * 0.2m);
                var wirelessMouse = _products["wirelessMouse"]!;
                decimal wirelessMousePrice = wirelessMouse.ListPrice + (wirelessMouse.ListPrice * 0.2m);
                return _rayaTechStore.CreateSaleOrder(
                    customerId: goldCustomer!.CustomerId,
                    items: [
                        (laptopPro15.ProductId, 2, laptopPro15Price),
                        (wirelessMouse.ProductId, 3, wirelessMousePrice),
                ]);
            }
        );
        _salesOrders["saleOrder01"] = saleOrder01;
        Console.WriteLine($"  {saleOrder01}");

        Do("Ship SaleOrder01 → WAREHOUSE-MAIN", () =>
        {
            var shipped = _rayaTechStore.ShipSaleOrder(saleOrder01!.OrderId);
            foreach (var item in shipped.Items) Console.WriteLine($"  {item}");
        });

        Do($"Deliver SaleOrder01 → {goldCustomer.Name}", () =>
        {
            var deliveredOrder = _rayaTechStore.DeliverSaleOrder(saleOrder01!.OrderId);
            Console.WriteLine($"  {deliveredOrder}");
        });

        Do("Mark SaleOrder01 As Paid", () => _rayaTechStore.PaySaleOrder(saleOrder01!.OrderId));

        return true;
    }
    private static bool ExecuteSaleCycleBySilverCustomer()
    {
        var silverCustomer = _customers.Values.FirstOrDefault(c => c!.Tier == CustomerTier.Silver);
        if(silverCustomer is null)
        {
            Helpers.PrintWarning("Opps, No Silver Customer Found");
            return false;
        }

        // SaleOrder02: Silver Customer buys 1 monitor + 2 keyboards (Silver 5% discount)
        var saleOrder02 = Add(
            $"SaleOrder02: {silverCustomer.Name} buys 1×Monitor + 2×Keyboard",
            () => {
                var monitor27 = _products["monitor27"]!;
                decimal monitor27Price = monitor27.ListPrice + (monitor27.ListPrice * 0.15m);
                var mechKeyboard = _products["mechKeyboard"]!;
                decimal mechKeyboardPrice = mechKeyboard.ListPrice + (mechKeyboard.ListPrice * 0.15m);
                return _rayaTechStore.CreateSaleOrder(
                    customerId: silverCustomer!.CustomerId,
                    items: [
                        (monitor27.ProductId, 1, monitor27Price),
                        (mechKeyboard.ProductId, 2, mechKeyboardPrice),
                ]);
            }
        );
        _salesOrders["saleOrder02"] = saleOrder02;
        Console.WriteLine($"  {saleOrder02}");

        Do("Ship + Deliver SaleOrder02", () =>
        {
            _rayaTechStore.ShipSaleOrder(saleOrder02!.OrderId);
            var deliveredOrder = _rayaTechStore.DeliverSaleOrder(saleOrder02!.OrderId);
            Console.WriteLine($"  {deliveredOrder}");
        });

        Do("Mark SaleOrder02 As Paid", () => _rayaTechStore.PaySaleOrder(saleOrder02!.OrderId));

        return true;
    }
    private static bool ExecuteSaleCycleByStandardCustomer()
    {
        var standardCustomer = _customers.Values.FirstOrDefault(c => c!.Tier == CustomerTier.Standard);
        if(standardCustomer is null)
        {
            Helpers.PrintWarning("Opps, No Standard Customer Found");
            return false;
        }

        // SaleOrder03: Standard Customer buys 3 chairs (no discount)
        var saleOrder03 = Add(
            $"SaleOrder03: {standardCustomer.Name} buys 3×Chair",
            () => {
                var ergonomicChair = _products["ergonomicChair"]!;
                decimal ergonomicChairPrice = ergonomicChair.ListPrice + (ergonomicChair.ListPrice * 0.25m);
                return _rayaTechStore.CreateSaleOrder(
                    customerId: standardCustomer!.CustomerId,
                    items: [(ergonomicChair.ProductId, 3, ergonomicChairPrice)]
                );
            }
        );
        _salesOrders["saleOrder03"] = saleOrder03;
        Console.WriteLine($"  {saleOrder03}");

        Do("Ship + Deliver SaleOrder03", () =>
        {
            _rayaTechStore.ShipSaleOrder(saleOrder03!.OrderId);
            _rayaTechStore.DeliverSaleOrder(saleOrder03!.OrderId);
        });

        Do("Mark SaleOrder03 paid", () => _rayaTechStore.PaySaleOrder(saleOrder03!.OrderId));

        return true;
    }
    private static bool ExecuteSaleCycleForBulkOrder()
    {
        var customer = Helpers.PickOne(_customers.Values);
        if(customer is null)
        {
            Helpers.PrintWarning("Opps, No Customer Found");
            return false;
        }
        // SaleOrder04: bulk order 90 keyboards — (80 @ $60) + (10 @ $58)
        // for tests FIFO across two keyboard cost layers
        var saleOrder04 = Add(
            "SaleOrder04 Sara: 90×Keyboard (spans 2 cost layers)",
            () => {
                var mechKeyboard = _products["mechKeyboard"]!;
                decimal mechKeyboardPrice = mechKeyboard.ListPrice + (mechKeyboard.ListPrice * 0.15m);
                return _rayaTechStore.CreateSaleOrder(
                    customerId: customer!.CustomerId,
                    items: [(mechKeyboard.ProductId, 90, mechKeyboardPrice)]
                );
            }
        );
        _salesOrders["saleOrder04"] = saleOrder04;
        Console.WriteLine($"  {saleOrder04}");

        Do("Ship + Deliver SO-4", () =>
        {
            _rayaTechStore.ShipSaleOrder(saleOrder04!.OrderId);
            _rayaTechStore.DeliverSaleOrder(saleOrder04!.OrderId);
        });

        Do("Mark SO-4 paid", () => _rayaTechStore.PaySaleOrder(saleOrder04!.OrderId));

        return true;
    }
    private static bool ExecuteSaleCycleForCancelledOrder()
    {
        var customer = Helpers.PickOne(_customers.Values);
        if(customer is null)
        {
            Helpers.PrintWarning("Opps, No Customer Found");
            return false;
        }

        // SaleOrder05: cancelled order (should NOT affect profits)
        var saleOrder05 = Add(
            $"SaleOrder05: {customer.Name} buys 90×Keyboard",
            () =>
            {
                var ryzen7 = _products["ryzen7"]!;
                decimal ryzen7Price = ryzen7.ListPrice + (ryzen7.ListPrice * 0.25m);
                return _rayaTechStore.CreateSaleOrder(
                    customerId: customer!.CustomerId,
                    items: [(ryzen7.ProductId, 5, ryzen7Price)]
                );
            }
        );
        _salesOrders["saleOrder05"] = saleOrder05;
        Console.WriteLine($"  {saleOrder05}");

        Do("Cancel SaleOrder05", () => _rayaTechStore.CancelSaleOrder(saleOrder05!.OrderId));

        return true;
    }
    private static bool ExecuteSaleCycleForReturnedOrder()
    {
        var customer = Helpers.PickOne(_customers.Values);
        if(customer is null)
        {
            Helpers.PrintWarning("Opps, No Customer Found");
            return false;
        }

        // SaleOrder06: return order after it was created, shipped and delivered
        var saleOrder06 = Add(
            $"SaleOrder06: {customer.Name} buys 1×Keyboard and 1xMouse",
            () =>
            {
                var mechKeyboard = _products["mechKeyboard"]!;
                decimal mechKeyboardPrice = mechKeyboard.ListPrice + (mechKeyboard.ListPrice * 0.15m);
                var wirelessMouse = _products["wirelessMouse"]!;
                decimal wirelessMousePrice = wirelessMouse.ListPrice + (wirelessMouse.ListPrice * 0.15m);
                return _rayaTechStore.CreateSaleOrder(
                    customerId: customer!.CustomerId,
                    items: [
                        (mechKeyboard.ProductId, 1, mechKeyboardPrice),
                        (wirelessMouse.ProductId, 1, wirelessMousePrice),
                    ]);
            }
        );
        _salesOrders["saleOrder06"] = saleOrder06;
        Console.WriteLine($"  {saleOrder06}");

        Do("Ship + Deliver SaleOrder06", () =>
        {
            _rayaTechStore.ShipSaleOrder(saleOrder06!.OrderId);
            _rayaTechStore.DeliverSaleOrder(saleOrder06!.OrderId);
        });

        Do("Return SaleOrder06 (stock returned)", () =>
        {
            var returnedOrder = _rayaTechStore.ReturnSaleOrder(saleOrder06!.OrderId);
            Console.WriteLine($"  {returnedOrder}");
        });

        return true;
    }

    private static void RunExceptionsScenarios()
    {
        // Trying To Add Duplicate Sku Product
        var product01 = Helpers.PickOne(_products.Values);
        Do(
            "Trying To Add Duplicate Sku Product -> Throws DuplicateSkuException",
            () => _rayaTechStore.AddNewProduct(
                sku: product01!.Sku,
                name: _faker.Commerce.ProductName(),
                description: _faker.Commerce.ProductDescription(),
                listPrice: _faker.Random.Decimal(20, 120),
                category: _faker.Random.Enum<ProductCategory>(),
                unit: product01.Unit
            )
        );

        // Trying To Sell Non-Existing Product
        Do(
            "Trying To Sell Non-Existing Product -> Throws ProductNotFoundException",
            () => _rayaTechStore.CreateSaleOrder(
                customerId: Helpers.PickOne(_customers.Values)!.CustomerId,
                items: [("PRODUCT-000000000", 5, _faker.Random.Decimal(20, 120))]
            )
        );

        // Trying To Purchase From Non-Existing Supplier
        Do(
            "Trying To Purchase From Non-Existing Supplier -> Throws SupplierNotFoundException",
            () => _rayaTechStore.CreatePurchaseOrder(
                supplierId: "SUPPLIER-000000",
                items: [(_products["ryzen7"]!.ProductId, 5, _products["ryzen7"]!.ListPrice)]
            )
        );

        // Trying To Sell To Non-Existing Customer
        Do(
            "Trying To Sell To Non-Existing Customer -> Throws CustomerNotFoundException",
            () => _rayaTechStore.CreateSaleOrder(
                customerId: "CUSTOMER-000000",
                items: [(_products["ryzen7"]!.ProductId, 5, _products["ryzen7"]!.ListPrice)]
            )
        );

        // Trying To Sell (Insufficient Stock)
        var customer = Helpers.PickOne(_customers.Values)!;
        var product02 = Helpers.PickOne(_products.Values)!;
        int quantityOrdered = _rayaTechStore.StockLevel(product02.ProductId) + 100;
        decimal product02Price = product02.ListPrice + (product02.ListPrice * 0.15m);
        Do(
            "Trying To Sell Out Of Stock -> Throws InsufficientStockException",
            () => {
                var saleOrder07 = _rayaTechStore.CreateSaleOrder(
                    customerId: customer.CustomerId,
                    items: [(product02.ProductId, quantityOrdered, product02Price)]
                );
                Console.WriteLine($"  {saleOrder07}");
                _salesOrders["saleOrder07"] = saleOrder07;
            }
        );

        // Trying To Ship Already-Cancelled saleOrder05
        Do(
            "Ship Already-Cancelled saleOrder05 -> Throws InvalidOrderStateException",
            () => _rayaTechStore.ShipSaleOrder(_salesOrders["saleOrder05"]!.OrderId)
        );

        // Trying To Deliver Already-Cancelled saleOrder05
        Do(
            "Deliver Already-Cancelled saleOrder05 -> Throws InvalidOrderStateException",
            () => _rayaTechStore.DeliverSaleOrder(_salesOrders["saleOrder05"]!.OrderId)
        );
    }

    public static void Run()
    {
        // ── START OF INVENTORY MANAGEMENT APP ──────────────────────────────────────────
        Helpers.PrintHeader("Start of Inventory Management App");

        // ── generate some products ─────────────────────────────────────────────────────
        Helpers.PrintSection("1 — Generate some Products");
        GenerateProducts();

        // ── generate some customers ────────────────────────────────────────────────────
        Helpers.PrintSection("2 — Generate some Customers");
        GenerateCustomers();

        // ── generate some suppliers ────────────────────────────────────────────────────
        Helpers.PrintSection("3 — Generate some Suppliers");
        GenerateSuppliers();

        // ── purchase orders cycle ─────────────────────────────────────────────────────
        Helpers.PrintSection("4 — Execute Some Purchases Orders Cycles");
        ExecuteAllProductPurchaseCycle();
        ExecuteRandomPurchasesCycles();

        // ── SNAPSHOT BEFORE SALES ─────────────────────────────────────────────────────
        Helpers.PrintSection("5 — Inventory Snapshot Before Any Sales");
        Console.WriteLine(_rayaTechStore.InventorySnapshot);

        // ── COSTS MAP BEFORE SALES ────────────────────────────────────────────────────
        Helpers.PrintSection("6 — Inventory Costs Map Before Any Sales");
        Console.WriteLine(_rayaTechStore.InventoryCostsMapString);

        // ── SALES CYCLE ───────────────────────────────────────────────────────────────
        Helpers.PrintSection("7 — Execute Some Sales Orders Cycles");
        ExecuteSaleCycleByGoldCustomer();
        ExecuteSaleCycleBySilverCustomer();
        ExecuteSaleCycleByStandardCustomer();
        ExecuteSaleCycleForBulkOrder();
        ExecuteSaleCycleForCancelledOrder();
        ExecuteSaleCycleForReturnedOrder();

        // ── SNAPSHOT AFTER SALES ──────────────────────────────────────────────────────
        Helpers.PrintSection("8 — Inventory Snapshot After Sales");
        Console.WriteLine(_rayaTechStore.InventorySnapshot);

        // ── EXCEPTIONS SCENARIOS ─────────────────────────────────────────────────────
        Helpers.PrintSection("9 — Exceptions Scenarios");
        RunExceptionsScenarios();

        // ── PROFIT REPORT ─────────────────────────────────────────────────────────────
        Helpers.PrintSection("10 — Full Profit & Loss Report");
        try { Console.WriteLine(_rayaTechStore.ProfitReportString()); }
        catch(Exception error) { Helpers.PrintError(error.Message); }

        // ── ALL PURCHASES ORDERS ──────────────────────────────────────────────────────
        Helpers.PrintSection("11 — All purchase orders");
        foreach (var po in _rayaTechStore.PurchasesOrders) Console.WriteLine($"  {po}");

        // ─── ALL SALES ORDERS ─────────────────────────────────────────────────────────
        Helpers.PrintSection("12 — All sales orders");
        foreach (var so in _rayaTechStore.SalesOrders) Console.WriteLine($"  {so}");

        // ── END OF APP ────────────────────────────────────────────────────────────────
        Helpers.PrintFooter("End of Inventory Management App");
    }
}
