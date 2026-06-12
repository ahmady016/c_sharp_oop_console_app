using System.Collections.Frozen;
using System.Text;
using System.Text.Json.Serialization;

namespace BankManagement;

public record AccountsPerType
(
    string AccountType,
    int Count,
    decimal TotalBalance
);
public record AccountsPerCustomer
(
    string CustomerId,
    int CheckingAccountsCount,
    decimal TotalBalance,
    int LoanAccountsCount,
    decimal TotalLoanBalance,
    decimal TotalOutstandingBalance
);

// ══════════════════════════════════════════════════════════════════════════════════════
//  BANK SYSTEM (COMPOSITION ROOT) FOR ACCOUNTS MANAGEMENT
// ══════════════════════════════════════════════════════════════════════════════════════
public sealed class Bank
{
    // ── encapsulated state (private fields) ──────────────────────────────────────────────
    private readonly string _name;
    private readonly string _subName;
    private readonly string _location;
    private readonly Dictionary<string, Customer> _customersMap = [];
    private readonly Dictionary<string, Account>  _accountsMap  = [];
    private readonly Dictionary<string, Transaction> _transfersMap = [];
    private int _accountSequence = 1000;

    // ── public Identity (getter properties) ──────────────────────────────────────────────
    public string BankName => _name;
    public string BankLocation => _location;
    public string FullName => $"{_name.Replace(" ", "_").ToLower()}_"+
        $"{_subName.Replace(" ", "_").ToLower()}_"+
        $"{_location.Replace(" ", "_").ToLower()}";
    public List<Customer> Customers => [.._customersMap.Values];
    public List<Account> Accounts  => [.._accountsMap.Values];
    public IReadOnlyList<Transaction> Transfers => [.._transfersMap.Values];

    // ── computed getter properties ─────────────────────────────────────────────────────
    public int CustomersCount => _customersMap.Count;
    public int AccountsCount => _accountsMap.Count;
    [JsonIgnore]
    public IReadOnlyList<Account> ActiveAccounts => [..
        from account in _accountsMap.Values
        where account.Status == AccountStatus.Active.ToString()
        orderby account.Balance descending
        select account
    ];
    [JsonIgnore]
    public IReadOnlyList<Account> FrozenAccounts => [..
        from account in _accountsMap.Values
        where account.Status == AccountStatus.Frozen.ToString()
        orderby account.Balance descending
        select account
    ];
    [JsonIgnore]
    public IReadOnlyList<Account> ClosedAccounts => [..
        from account in _accountsMap.Values
        where account.Status == AccountStatus.Closed.ToString()
        orderby account.Balance descending
        select account
    ];
    [JsonIgnore]
    public IReadOnlyList<CheckingAccount> DepositAccounts => [..
        from account in _accountsMap.Values.OfType<CheckingAccount>()
        orderby account.Balance descending
        select account
    ];
    public decimal TotalDeposits => Math.Round(DepositAccounts.Sum(a => a.Balance), 2, MidpointRounding.AwayFromZero);
    [JsonIgnore]
    public IReadOnlyList<LoanAccount> LoanAccounts => [..
        from account in _accountsMap.Values.OfType<LoanAccount>()
        orderby account.Balance descending
        select account
    ];
    public decimal TotalLoans => Math.Round(LoanAccounts.Sum(a => a.Balance), 2, MidpointRounding.AwayFromZero);
    public FrozenDictionary<string, AccountsPerType> AccountsPerType => Accounts.Aggregate(
        new Dictionary<string, AccountsPerType>() {
            { "Checking Account", new AccountsPerType("Checking", 0, 0) },
            { "Loan Account", new AccountsPerType("Loan", 0, 0) }
        },
        (dict, account) => {
            var type = account.AccountType;
            var existed = dict[type];
            dict[type] = existed with {
                Count = existed.Count + 1,
                TotalBalance = existed.TotalBalance + account.Balance
            };
            return dict;
        },
        dict => dict.ToFrozenDictionary()
    );
    public FrozenDictionary<string, AccountsPerCustomer> AccountsPerCustomer => Accounts.Aggregate(
        (
            from customerId in _customersMap.Keys
            select KeyValuePair.Create(
                customerId,
                new AccountsPerCustomer(
                    CustomerId: customerId,
                    CheckingAccountsCount: 0,
                    TotalBalance: 0,
                    LoanAccountsCount: 0,
                    TotalLoanBalance: 0,
                    TotalOutstandingBalance: 0
                )
            )
        ).ToDictionary(),
        (dict, account) => {
            var customerId = account.CustomerId;
            var existed = dict[customerId];
            dict[customerId] = account switch
            {
                CheckingAccount checkingAccount => existed with
                {
                    CheckingAccountsCount = existed.CheckingAccountsCount + 1,
                    TotalBalance = existed.TotalBalance + checkingAccount.Balance
                },
                LoanAccount loanAccount => existed with
                {
                    LoanAccountsCount = existed.LoanAccountsCount + 1,
                    TotalLoanBalance = existed.TotalLoanBalance + loanAccount.Balance,
                    TotalOutstandingBalance = existed.TotalOutstandingBalance + loanAccount.Balance
                },
                _ => existed
            };
            return dict;
        },
        dict => dict.ToFrozenDictionary()
    );

    // ── reporting ──────────────────────────────────────────────────────────────────────
    public string PortfolioSummary
    {
        get
        {
            var reportBuilder = new StringBuilder();
            string divider = new('═', 80);

            reportBuilder.AppendLine(divider);
            reportBuilder.AppendLine($"——— PORTFOLIO SUMMARY ———");
            reportBuilder.AppendLine($"  {BankName.ToUpper()} | {BankLocation}");
            reportBuilder.AppendLine($"  Customers: ({CustomersCount}) | Transfers: ({Transfers.Count})");
            reportBuilder.AppendLine(
                $"  Accounts: ({AccountsCount}) -> ({ActiveAccounts.Count}) Active | " +
                $"({FrozenAccounts.Count}) Frozen | ({ClosedAccounts.Count}) Closed | " +
                $"({DepositAccounts.Count}) Deposits | ({LoanAccounts.Count}) Loans"
            );
            reportBuilder.AppendLine(divider);

            reportBuilder.AppendLine($"  Total deposits : {TotalDeposits,12:C}");
            reportBuilder.AppendLine($"  Total loans    : {TotalLoans,12:C}");
            reportBuilder.AppendLine($"  Net position   : {TotalDeposits - Math.Abs(TotalLoans),12:C}");
            reportBuilder.AppendLine();

            reportBuilder.AppendLine($"  {"Account Type",-22} {"Count",5} {"Total Balance",14}");
            reportBuilder.AppendLine($"  {new string('─', 44)}");

            foreach (var (type, stats) in AccountsPerType)
                reportBuilder.AppendLine($"  {type,-22} {stats.Count,5} {stats.TotalBalance,14:C}");

            reportBuilder.AppendLine(divider);
            return reportBuilder.ToString();
        }
    }

    // ── public factory constructor ─────────────────────────────────────────────────────
    [JsonConstructor]
    public Bank(string name, string subName, string location)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("must provide a valid name", nameof(name));
        if(string.IsNullOrWhiteSpace(subName))
            throw new ArgumentException("must provide a valid subname", nameof(subName));
        if(string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("must provide a valid location", nameof(location));

        _name = name;
        _subName = subName;
        _location = location;
    }

    // ── override ToString ───────────────────────────────────────────────────────────────
    public override string ToString() =>
        $"{BankName} located at {BankLocation}, has ({Customers.Count}) customers and ({Accounts.Count}) accounts";

    // ── record a transfer transaction ────────────────────────────────────────────────────
    private void RecordTransfer(
        decimal amount,
        decimal balanceAfter,
        string message
    )
    {
        Transaction newTransaction = new(
            type: TransactionType.Transfer.ToString(),
            amount: amount,
            balanceAfter: balanceAfter,
            description: message
        );
        _transfersMap[newTransaction.Id] = newTransaction;
    }
    // ── next accountId and customerId ────────────────────────────────────────────────────
    private string NextCustomerId() => $"CUSTOMER-{_customersMap.Count + 1:000000000000}";
    private string NextAccountId(string prefix) => $"{prefix}-{++_accountSequence:000000000000}";

    // ── get customer and account by id ─────────────────────────────────────────────────
    public Customer GetCustomer(string customerId) =>
        _customersMap.TryGetValue(customerId, out var existedCustomer)
            ? existedCustomer
            : throw new CustomerNotFoundException(customerId);
    public Account GetAccount(string accountId) =>
        _accountsMap.TryGetValue(accountId, out var existedAccount)
            ? existedAccount
            : throw new AccountNotFoundException(accountId);
    public T GetAccount<T>(string accountId) where T : Account
    {
        var existedAccount = GetAccount(accountId);
        return existedAccount as T
            ?? throw new AccountCastException(
                accountId,
                $"Invalid Cast -> ({accountId}) Account is ({existedAccount.GetType().Name}), not ({typeof(T).Name})."
            );
    }

    // ── register new customer ───────────────────────────────────────────────────────────
    public Customer RegisterCustomer(
        string firstName,
        string lastName,
        string nationalId,
        string mobile,
        string email,
        string birthDate,
        string gender
    )
    {
        var existedCustomer = GetCustomerByNationalId(nationalId);
        if (existedCustomer is not null)
            throw new CustomerAlreadyExistsException(existedCustomer.Id, $"Customer with this nationalId ({nationalId}) already exists.");

        var customerId = NextCustomerId();
        var newCustomer  = new Customer(
            id: customerId,
            firstName: firstName,
            lastName: lastName,
            birthDate: birthDate,
            gender: gender,
            email: email,
            mobileNumber: mobile,
            nationalId: nationalId
        );
        _customersMap[customerId] = newCustomer;
        Console.WriteLine(
            $"({BankName}) -> customer #{_customersMap.Count:00} | " +
            $"({customerId}) | ({newCustomer.FullName}) is registered successfully."
        );
        return newCustomer;
    }

    // ── account factory methods ─────────────────────────────────────────────────────────
    public CheckingAccount OpenCheckingAccount(
        string customerId,
        decimal initialDeposit,
        decimal overdraftLimit = 1000m
    )
    {
        if(!_customersMap.ContainsKey(customerId))
            throw new CustomerNotFoundException(customerId);

        var accountId  = NextAccountId("CHECKING");
        var newAccount = new CheckingAccount(
            accountId: accountId,
            customerId: customerId,
            ownerName: GetCustomer(customerId).FullName,
            initialDeposit: initialDeposit,
            overdraftLimit: overdraftLimit
        );
        _accountsMap[accountId] = newAccount;
        return newAccount;
    }

    public LoanAccount OpenLoanAccount(
        string customerId,
        decimal totalAmount,
        decimal paymentAmount
    )
    {
        if(!_customersMap.ContainsKey(customerId))
            throw new CustomerNotFoundException(customerId);

        var accountId  = NextAccountId("LOAN");
        var newAccount = new LoanAccount(
            accountId: accountId,
            customerId: customerId,
            ownerName: GetCustomer(customerId).FullName,
            totalAmount: totalAmount,
            paymentAmount: paymentAmount
        );
        _accountsMap[accountId] = newAccount;
        return newAccount;
    }

    // ── transfer between accounts ───────────────────────────────────────────────────────
    public void Transfer(string fromId, string toId, decimal amount)
    {
        if(string.IsNullOrWhiteSpace(fromId) || string.IsNullOrWhiteSpace(toId))
            throw new ArgumentException("from and to accounts ids must be provided.");
        if(amount <= 0)
            throw new InvalidAmountException(fromId, amount);
        if(fromId == toId)
            throw new InvalidTransferException(fromId);

        var sender = GetAccount<CheckingAccount>(fromId);
        var receiver = GetAccount<CheckingAccount>(toId);
        sender.Withdraw(
            amount,
            $"Transfer To ({toId}) Account at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
        );
        receiver.Deposit(
            amount,
            $"Transfer From ({fromId}) Account at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
        );
        RecordTransfer(
            amount: amount,
            balanceAfter: receiver.Balance,
            message: $"Transfer of {amount:C2} from ({fromId}) to ({toId}) at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
        );
    }

    // ── bank-wide operations (polymorphism in action) ─────────────────────────────────────
    public void RunMonthlyProcessing()
    {
        foreach (var account in ActiveAccounts)
        {
            switch (account)
            {
                case CheckingAccount checkingAccount:
                    checkingAccount.ChargeMonthlyFee();
                    break;
                case LoanAccount loanAccount:
                    loanAccount.ApplyMonthlyPayment();
                    break;
            }
        }
    }

    // ── customer accounts listing ───────────────────────────────────────────────────────
    public IReadOnlyList<Account> GetCustomerAccounts(string customerId) => [..
        from account in Accounts
        where account.CustomerId == customerId
        select account
    ];
    public Customer? GetCustomerByNationalId(string nationalId) =>
        Customers.FirstOrDefault(c => c.NationalId == nationalId);

}
