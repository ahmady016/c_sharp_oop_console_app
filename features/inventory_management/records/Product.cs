namespace InventoryManagement;

public record ProductFullName(
    string ProductId,
    string Sku,
    string Name
)
{
    public string Value => $"{Sku.ToLower()}_{Name.Replace(" ", "_").ToLower()}";
    public override string ToString() => Value;
}

public record Product(
    string          ProductId,
    string          Sku,
    string          Name,
    string          Description,
    decimal         ListPrice, // recommended selling price
    ProductCategory Category,
    string          Unit // unit of measure (pcs, kg, litres)
)
{
    public string FullName => $"{Sku.ToLower()}_{Name.Replace(" ", "_").ToLower()}";
    public override string ToString()
        => $"[{Sku}] {Name,-26} {Category,-14} List: {ListPrice,8:C}";
}
