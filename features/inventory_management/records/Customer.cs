namespace InventoryManagement;

public record Customer(
    string       CustomerId,
    string       Name,
    string       Email,
    string       Phone,
    CustomerTier Tier,
    decimal      DiscountRate   // e.g. 0.05 = 5%
)
{
    public string FullName => $"{CustomerId}_{Name.Replace(' ', '_').ToLower()}";
    public override string ToString()
        => $"[{CustomerId}] {Name,-22} {Tier,-10} Discount: {DiscountRate:P0}";
}
