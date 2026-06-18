namespace InventoryManagement;

public record Supplier(
    string SupplierId,
    string Name,
    string ContactEmail,
    string Mobile,
    string PaymentTerms   // e.g. "Net30", "Net60"
)
{
    public override string ToString()
        => $"[{SupplierId}] {Name,-22} {ContactEmail,-28} Terms: {PaymentTerms}";
}
