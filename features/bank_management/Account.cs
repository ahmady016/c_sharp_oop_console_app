using System.Text;
using System.Text.Json.Serialization;

namespace BankManagement;

public enum AccountStatus : byte { Active = 1, Frozen = 2, Closed = 3 }

// ══════════════════════════════════════════════════════════════════════════════════════
//  ABSTRACT ACCOUNT — base of the hierarchy
// ══════════════════════════════════════════════════════════════════════════════════════
public abstract class Account
{
    // ── encapsulated state (private fields) ──────────────────────────────────────────────
    private readonly string _accountId;
    private readonly string _customerId;
    private readonly string _ownerName;
    private readonly DateTime _createdAt;
    private decimal _balance;
    private AccountStatus _status;
    private readonly Dictionary<string, Transaction> _transactionsMap = [];

    // ── public Identity (getter properties) ──────────────────────────────────────────────
    public string AccountId => _accountId;
    public string CustomerId => _customerId;
    public string OwnerName => _ownerName;
    public DateTime CreatedAt => _createdAt;
    public decimal Balance => _balance;
    public string Status => _status.ToString();
    public IReadOnlyList<Transaction> Transactions => [.._transactionsMap.Values];

    // ── abstract: each account type defines its own statement header ───────────────────
    public abstract string StatementHeader();

    // ── abstract: each account type defines its own type ──────────────────────────────
    public abstract string AccountType  { get; }

    // ── statement printing (shared pipeline) ──────────────────────────────────────────
    public string Statement
    {
        get
        {
            var sb = new StringBuilder();
            string divider = new('─', 80);

            sb.AppendLine(divider);
            sb.AppendLine(StatementHeader());
            sb.AppendLine(
                $"  AccountId: {AccountId} | Owner: {OwnerName}" +
                $"  OpenedAt: {CreatedAt:yyyy-MM-dd hh:mm tt}" +
                $"  Status: {Status} | Transactions: {Transactions.Count}"
            );
            sb.AppendLine($"  Balance : {_balance:C}");
            sb.AppendLine(divider);

            if (Transactions.Count > 0)
                foreach (var t in Transactions.OrderBy(t => t.Timestamp))
                    sb.AppendLine(t.ToString());
            else
                sb.AppendLine("  No transactions recorded.");

            sb.AppendLine(divider);
            return sb.ToString();
        }
    }

    // ── protected factory constructor ──────────────────────────────────────────────────
    [JsonConstructor]
    protected Account(
        string accountId,
        string customerId,
        string ownerName,
        decimal initialDeposit
    )
    {
        _accountId = accountId;
        _customerId = customerId;
        _ownerName = ownerName;
        _balance = initialDeposit;
        _createdAt = DateTime.Now;
        _status = AccountStatus.Active;
    }

    // ── override ToString ───────────────────────────────────────────────────────────────
    public override string ToString() =>
        $"[{AccountId}] -> {AccountType,-20} Owner: {OwnerName,-20} | Balance: {_balance,12:C} | Status: {Status}";

    // ── helper method ────────────────────────────────────────────────────────────────
    protected void RecordTransaction(string type, decimal amount, string description)
    {
        Transaction newTransaction = new(type, amount, _balance, description);
        _transactionsMap[newTransaction.Id] = newTransaction;
    }

    // ── public operations ──────────────────────────────────────────────────────────────
    public void Freeze()
    {
        if(_status == AccountStatus.Frozen)
            throw new AccountInvalidOperationException(AccountId, $"Account ({AccountId}) is already frozen.");
        if(_status == AccountStatus.Closed)
            throw new AccountInvalidOperationException(AccountId, $"Account ({AccountId}) is closed.");
        _status = AccountStatus.Frozen;
    }
    public void Activate()
    {
        if(_status == AccountStatus.Active)
            throw new AccountInvalidOperationException(AccountId, $"Account ({AccountId}) is already active.");
        if(_status == AccountStatus.Closed)
            throw new AccountInvalidOperationException(AccountId, $"Account ({AccountId}) is closed.");
        _status = AccountStatus.Active;
    }
    public void Close()
    {
        if(Balance != 0)
            throw new AccountNotEmptyException(AccountId, Balance);
        if(_status == AccountStatus.Frozen)
            throw new AccountFrozenException(AccountId);
        if(_status == AccountStatus.Closed)
            throw new AccountInvalidOperationException(AccountId, $"Account ({AccountId}) is already closed.");
        _status = AccountStatus.Closed;
    }

    // exposed and protected for balance adjustments in derived classes
    protected void AdjustBalance(decimal delta, string type, string desc)
    {
        _balance += delta;
        RecordTransaction(type, Math.Abs(delta), desc);
    }

}
