using System.Text.Json.Serialization;

namespace BankManagement;

// ═════════════════════════════════════════════════════════════════════════════════════
//  TRANSACTION — record each operation performed on an account
// ═════════════════════════════════════════════════════════════════════════════════════
public enum TransactionType : byte
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Repayment = 4,
    Fee = 5
}

public sealed class Transaction
{
    // ── encapsulated state (private fields) ───────────────────────────────────────────────
    private readonly string _id;
    private readonly TransactionType _type;
    private readonly DateTime _timestamp;
    private readonly decimal _amount;
    private readonly decimal _balanceAfter;
    private readonly string _description;

    // ── public Identity (getter properties) ───────────────────────────────────────────────
    public string Id => _id;
    public string Type => _type.ToString();
    public string Timestamp => $"{_timestamp:yyyy-MM-dd hh:mm:ss tt}";
    public decimal Amount => _amount;
    public decimal BalanceAfter => _balanceAfter;
    public string Description => _description;

    // ── public factory constructor ──────────────────────────────────────────────────────
    [JsonConstructor]
    public Transaction(
        string type,
        decimal amount,
        decimal balanceAfter,
        string description,
        string? id = null,
        string? timestamp = null
    )
    {
        if (string.IsNullOrWhiteSpace(type) || !Enum.TryParse(type, out TransactionType parsedType))
            throw new ArgumentException("Transaction type cannot be null or whitespace.", nameof(type));
        if(amount < 0)
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));
        if(string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
        DateTime parsedTimestamp = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(timestamp) && !DateTime.TryParse(timestamp, out parsedTimestamp))
            throw new ArgumentException("must provide a valid timestamp.", nameof(timestamp));

        _id = id ?? Guid.NewGuid().ToString();
        _type = parsedType;
        _timestamp = parsedTimestamp;
        _amount = amount;
        _balanceAfter = balanceAfter;
        _description = description;
    }

    // ── overrides ToString ──────────────────────────────────────────────────────────────
    public override string ToString() =>
        $"  {Timestamp:dd MMM yyyy HH:mm}  " +
        $"{Type,-12}  " +
        $"{(_type is TransactionType.Deposit ? "+" : "-"),1}" +
        $"{Amount,10:C}  " +
        $"Balance: {BalanceAfter,10:C}  " +
        $"{Description}";

}
