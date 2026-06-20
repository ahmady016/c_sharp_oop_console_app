namespace InventoryManagement;

public interface ISupplierRegistry
{
    IReadOnlyList<Supplier> All { get; }
    Supplier Add(string name, string email, string mobile, string terms);
    Supplier Find(string supplierId);
}

public sealed class SupplierRegistry : ISupplierRegistry
{
    private int _seq = 0;
    private readonly Dictionary<string, Supplier> _suppliersMap = [];
    public IReadOnlyList<Supplier> All => [.._suppliersMap.Values.OrderBy(s => s.Name)];

    public Supplier Add(string name, string email, string mobile, string terms)
    {
        var newSupplier = new Supplier($"SUPPLIER-{++_seq:D6}", name, email, mobile, terms);
        _suppliersMap[newSupplier.SupplierId] = newSupplier;
        return newSupplier;
    }

    public Supplier Find(string id) =>
        _suppliersMap.TryGetValue(id, out var existedSupplier)
            ? existedSupplier
            : throw new SupplierNotFoundException(id);

}
