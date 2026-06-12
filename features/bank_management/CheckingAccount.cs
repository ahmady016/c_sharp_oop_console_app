namespace BankManagement;

// ═══════════════════════════════════════════════════════════════════════════════════════
//  CHECKING ACCOUNT
// ═══════════════════════════════════════════════════════════════════════════════════════
public sealed class CheckingAccount : Account
{
    // ── encapsulated state (private fields) ──────────────────────────────────────────────
    private readonly decimal _overdraftLimit;
    private readonly decimal _monthlyFee;

    // ── public Identity (getter properties) ──────────────────────────────────────────────
    public decimal OverdraftLimit => _overdraftLimit;
    public decimal MonthlyFee     => _monthlyFee;

    // ── public factory constructor ────────────────────────────────────────────────────
    public CheckingAccount(
        string accountId,
        string customerId,
        string ownerName,
        decimal initialDeposit,
        decimal overdraftLimit = 1000m,
        decimal monthlyFee     = 20m
    ) : base(accountId, customerId, ownerName, initialDeposit)
    {
        if (initialDeposit <= 0)
            throw new InvalidAmountException(accountId, initialDeposit);

        _overdraftLimit = overdraftLimit;
        _monthlyFee     = monthlyFee;

        RecordTransaction(
            TransactionType.Deposit.ToString(),
            initialDeposit,
            $"{AccountType} Account opened with initial deposit {initialDeposit:C2} at {CreatedAt:yyyy-MM-dd hh:mm tt}"
        );
    }

    // ── override: AccountType and StatementHeader ────────────────────────────────────────
    public override string AccountType => "Checking Account";
    public override string StatementHeader() =>
        $"  CHECKING ACCOUNT STATEMENT  |  " +
        $"Overdraft limit: {OverdraftLimit:C}  |  Monthly fee: {MonthlyFee:C}";

    // ── validation on withdrawal and deposit ─────────────────────────────────────────────
    public void ValidateWithdrawal(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException(AccountId, amount);
        if (Status == AccountStatus.Frozen.ToString())
            throw new AccountFrozenException(AccountId);
        if (Balance - amount < -OverdraftLimit)
            throw new InsufficientFundsException(AccountId, Balance + OverdraftLimit, amount);
    }
    public void ValidateDeposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException(AccountId, amount);
        if (Status == AccountStatus.Frozen.ToString())
            throw new AccountFrozenException(AccountId);
    }

    // ── public operations ──────────────────────────────────────────────────────────────
    public void Deposit(decimal amount, string? description)
    {
        ValidateDeposit(amount);
        AdjustBalance(
            amount,
            TransactionType.Deposit.ToString(),
            description ?? $"Deposit of {amount:C2} at {CreatedAt:yyyy-MM-dd hh:mm tt}"
        );
    }
    public void Withdraw(decimal amount,string? description)
    {
        ValidateWithdrawal(amount);
        AdjustBalance(
            -amount,
            TransactionType.Withdrawal.ToString(),
            description ?? $"Withdrawal of {amount:C2} at {CreatedAt:yyyy-MM-dd hh:mm tt}"
        );
    }
    public void ChargeMonthlyFee()
    {
        AdjustBalance(-MonthlyFee, TransactionType.Fee.ToString(), $"Monthly account fee");
    }

}
