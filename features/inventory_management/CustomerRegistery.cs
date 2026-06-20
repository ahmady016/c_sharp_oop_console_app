namespace InventoryManagement;

public interface ICustomerRegistry
{
    public IReadOnlyList<Customer> All { get; }
    public Customer Add(
        string name,
        string email,
        string mobile,
        CustomerTier tier = CustomerTier.Standard,
        decimal discountRate = 0m
    );
    public Customer Find(string id);
}

public sealed class CustomerRegistry : ICustomerRegistry
{
    private int _seq = 0;
    private readonly Dictionary<string, Customer> _customersMap = [];
    public IReadOnlyList<Customer> All => [.._customersMap.Values.OrderBy(c => c.Name)];

    public Customer Add(
        string name,
        string email,
        string mobile,
        CustomerTier tier = CustomerTier.Standard,
        decimal discountRate = 0m
    )
    {
        var newCustomer = new Customer(
            CustomerId: $"CUS-{++_seq:D6}",
            Name: name,
            Email: email,
            Phone: mobile,
            Tier: tier,
            DiscountRate: discountRate
        );
        _customersMap[newCustomer.CustomerId] = newCustomer;
        return newCustomer;
    }

    public Customer Find(string id) =>
        _customersMap.TryGetValue(id, out var existedCustomer)
            ? existedCustomer
            : throw new CustomerNotFoundException(id);

}
