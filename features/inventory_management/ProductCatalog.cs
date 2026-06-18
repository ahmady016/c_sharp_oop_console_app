namespace InventoryManagement;

public interface IProductCatalog
{
    IReadOnlyList<Product> All { get; }
    Product Add(
        string sku,
        string name,
        string description,
        decimal listPrice,
        ProductCategory category,
        string unit = "pcs"
    );
    Product FindById(string productId);
    Product? FindBySku(string sku);
    IReadOnlyList<Product> Search(string keyword);
}

public sealed class ProductCatalog : IProductCatalog
{
    private int _seq = 0;
    private readonly Dictionary<string, string>  _skusMap = [];
    private readonly Dictionary<string, Product> _productsMap = [];
    public IReadOnlyList<Product> All => [.._productsMap.Values.OrderBy(p => p.Name)];

    public Product Add(
        string sku,
        string name,
        string description,
        decimal listPrice,
        ProductCategory category,
        string unit = "pcs"
    )
    {
        sku = sku.ToUpper();
        if (_skusMap.ContainsKey(sku))
            throw new DuplicateSkuException(sku);

        var id = $"PRODUCT-{++_seq:D9}";
        var newProduct = new Product(
            ProductId: id,
            Sku: sku,
            Name: name,
            Description: description,
            ListPrice: listPrice,
            Category: category,
            Unit: unit
        );
        _skusMap[sku] = id;
        _productsMap[id] = newProduct;
        return newProduct;
    }

    public Product FindById(string productId) =>
        _productsMap.TryGetValue(productId, out var existedProduct)
            ? existedProduct
            : throw new ProductNotFoundException(productId);

    public Product? FindBySku(string sku) =>
        _skusMap.TryGetValue(sku.ToUpper(), out var productId)
            ? _productsMap[productId]
            : null;

    private static bool IsMatch(Product product, string keyword) =>
        product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
        product.Sku.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    public IReadOnlyList<Product> Search(string keyword) => [..
        from product in _productsMap.Values
        where IsMatch(product, keyword)
        orderby product.Name
        select product
    ];

}
