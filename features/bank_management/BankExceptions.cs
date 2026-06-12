namespace BankManagement;

// ══════════════════════════════════════════════════════════════════════════════════════
//  BANK EXCEPTION BASE CLASS FOR ALL BANK EXCEPTIONS
// ══════════════════════════════════════════════════════════════════════════════════════
public abstract class BankException : Exception
{
    private readonly string _accountId;
    public string AccountId => _accountId;
    public BankException(
        string message,
        string accountId,
        Exception? inner = null
    ) : base(message, inner) { _accountId = accountId; }
}

public sealed class InsufficientFundsException : BankException
{
    private readonly decimal _available;
    private readonly decimal _requested;
    public decimal Available => _available;
    public decimal Requested => _requested;
    public InsufficientFundsException(
        string accountId,
        decimal available,
        decimal requested
    ) : base(
        $"Insufficient funds. Requested: {requested:C}, Available: {available:C}",
        accountId
    )
    {
        _available = available;
        _requested = requested;
    }
}

public sealed class InvalidAmountException : BankException
{
    private readonly decimal _amount;
    public decimal Amount => _amount;
    public InvalidAmountException(
        string accountId,
        decimal amount
    ) : base($"Invalid amount {amount:C}. Amount must be greater than zero.", accountId)
    { _amount = amount; }
}

public sealed class AccountFrozenException : BankException
{
    public AccountFrozenException(string accountId)
        : base($"Account {accountId} is frozen.", accountId) { }
}

public sealed class AccountNotFoundException : BankException
{
    public AccountNotFoundException(string accountId)
        : base($"Account {accountId} was not found.", accountId) { }
}

public sealed class AccountNotEmptyException : BankException
{
    private readonly decimal _balance;
    public decimal Balance => _balance;
    public AccountNotEmptyException(
        string accountId,
        decimal balance
    ) : base($"Account ({accountId}) is not empty. Balance: {balance:C2}", accountId)
    { _balance = balance; }
}

public sealed class AccountInvalidOperationException : BankException
{
    public AccountInvalidOperationException(string accountId, string message)
        : base(message, accountId) { }
}

public sealed class AccountCastException : BankException
{
    public AccountCastException(string accountId, string message)
        : base(message, accountId) { }
}

public sealed class MinimumBalanceException : BankException
{
    private readonly decimal _minimum;
    public decimal MinimumRequired => _minimum;
    public MinimumBalanceException(
        string accountId,
        decimal minimum
    ) : base(
        $"Withdrawal would breach minimum balance of {minimum:C}.",
        accountId
    )
    { _minimum = minimum; }
}

public sealed class InsufficientPaymentException : BankException
{
    private readonly decimal _paymentAmount;
    private readonly decimal _minimumPayment;
    public decimal PaymentAmount => _paymentAmount;
    public decimal MinimumPayment => _minimumPayment;
    public InsufficientPaymentException(
        string accountId,
        decimal paymentAmount,
        decimal minimumPayment
    ) : base(
        $"Insufficient payment. Pay: {paymentAmount:C2}, Over Minimum Payment: {minimumPayment:C2}",
        accountId
    )
    {
        _paymentAmount = paymentAmount;
        _minimumPayment = minimumPayment;
    }
}

public sealed class InvalidTransferException : BankException
{
    public InvalidTransferException(string accountId)
        : base($"Cannot transfer to the same account ({accountId}).", accountId) { }
}

public sealed class CustomerAlreadyExistsException : BankException
{
    public CustomerAlreadyExistsException(string customerId, string message = "")
        : base(message ?? $"Customer ({customerId}) already exists.", customerId) { }
}

public sealed class CustomerNotFoundException : BankException
{
    public CustomerNotFoundException(string customerId)
        : base($"Customer {customerId} was not found.", customerId) { }
}
