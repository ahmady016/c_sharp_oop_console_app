namespace BankManagement;

public sealed class LoanAccount : Account
{
    // ── encapsulated state (private fields) ──────────────────────────────────────────────
    private readonly DateTime _loanedAt;
    private readonly decimal _originalAmount;
    private readonly decimal _paymentAmount;

    // ── public Identity (getter properties) ──────────────────────────────────────────────
    public DateTime LoanedAt => _loanedAt;
    public decimal OriginalAmount => _originalAmount;
    public decimal PaymentAmount => _paymentAmount;
    public decimal MinPayment => PaymentAmount / 2;
    public int TotalMonths => (int)Math.Ceiling(OriginalAmount / PaymentAmount);
    public int RemainingMonths => (int)Math.Ceiling(Math.Abs(Balance) / PaymentAmount);

    // ── public factory constructor ──────────────────────────────────────────────────────
    public LoanAccount(
        string accountId,
        string customerId,
        string ownerName,
        decimal totalAmount,
        decimal paymentAmount
    ) : base(accountId, customerId, ownerName, -totalAmount)
    {
        _loanedAt = DateTime.Now;
        _originalAmount = totalAmount;
        _paymentAmount = paymentAmount;

        RecordTransaction(
            TransactionType.Withdrawal.ToString(),
            totalAmount,
            $"Loan disbursement of {OriginalAmount:C2} at {LoanedAt:yyyy-MM-dd hh:mm tt}"
        );
    }

    // ── override: AccountType and StatementHeader ────────────────────────────────────────
    public override string AccountType => "Loan Account";
    public override string StatementHeader() =>
        $"  LOAN ACCOUNT STATEMENT  |  " +
        $"Original Amount: {OriginalAmount:C}  |  " +
        $"Monthly Payment: {PaymentAmount:C2} | For {TotalMonths} months  |  " +
        $"Remaining: {Balance:C2} | For {RemainingMonths} months";

    // ── calculate - actual amount to be paid ────────────────────────────────────────────
    private decimal ActualPayment(decimal amount) => (Balance + amount) > 0
        ? amount - (Balance + PaymentAmount)
        : amount;

    // ── public operations - repayment reduces the debt ──────────────────────────────────
    public void MakeRepayment(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException(AccountId, amount);
        if (amount < MinPayment)
            throw new InsufficientPaymentException(AccountId, amount, MinPayment);

        AdjustBalance(
            ActualPayment(amount),
            TransactionType.Repayment.ToString(),
            $"Loan repayment of {ActualPayment:C2} at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
        );
    }
    public void ApplyMonthlyPayment() => MakeRepayment(PaymentAmount);

}
