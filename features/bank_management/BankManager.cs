/*
// ╔══════════════════════════════════════════════════════════════════════════╗
// ║          BANK MANAGEMENT SYSTEM — C# OOP Design                            ║                                                         ║
// ║  Demonstrates:                                                             ║
// ║    • Abstraction    — abstract Account, abstract BankException             ║
// ║    • Inheritance    — Checking / Loan extend Account                       ║
// ║    • Encapsulation  — private balance, controlled via methods              ║
// ║    • Polymorphism   — List<Account> dispatches to the right type           ║
// ║    • Composition    — Bank Class owns Customers + Accounts                 ║
// ╚══════════════════════════════════════════════════════════════════════════╝
# allow customers to create an account
# allow customers to deposit money into their account
# allow customers to withdraw money from their account
# allow customers to transfer money to another account
# allow customers to granted a loan (open a loan account)
# allow customers to pay back a loan (repay a loan account till it's fully paid)
# allow customers to check their account balance
# allow customers to view their transaction history
# allow customers to close their account
# note: load account closed only when there is no outstanding balance
# note: checking account closed only when the balance is $0
*/
using Bogus;
using Bogus.Extensions.Belgium;

namespace BankManagement;

// ═════════════════════════════════════════════════════════════════════════════════════
//  BANK MANAGER ── FOR TESTING BANK ACCOUNTS OPERATIONS
// ═════════════════════════════════════════════════════════════════════════════════════
public static class BankManager
{
    private static readonly Faker _faker = new();
    private static readonly string[] _accountTypes = ["CHECKING", "LOAN"];
    private static readonly string[] _transactionTypes = ["DEPOSIT", "WITHDRAWAL", "TRANSFER", "LOAN_REPAYMENT"];
    private static readonly string DATA_DIRECTORY = Path.Combine(Helpers.SameDirectory(), "data");

    private static void Do(string message, Action action)
    {
        try
        {
            Helpers.RunScenario(message, action);
            Helpers.PrintSuccess("Action Done Successfully...");
        }
        catch (BankException ex) { Helpers.PrintWarning(ex.Message); }
        catch (Exception ex) { Helpers.PrintError(ex.Message); }
    }

    private static List<Customer> RegisterCustomers(Bank bank, int count)
    {
        List<Customer> customers = [];
        for (int i = 0; i < count; i++)
        {
            var gender = _faker.Person.Gender;
            customers.Add(bank.RegisterCustomer(
                firstName: _faker.Name.FirstName(gender),
                lastName: _faker.Name.LastName(gender),
                nationalId: Helpers.GenerateDigitsOnlyId(14),
                mobile: _faker.Phone.PhoneNumber("01#########"),
                email: _faker.Person.Email,
                birthDate: _faker.Date.BetweenDateOnly(
                    start: DateOnly.Parse("1974-01-01"),
                    end: DateOnly.Parse("2004-12-31")
                ).ToString("yyyy-MM-dd"),
                gender: gender.ToString().ToLower()
            ));
        }
        return customers;
    }

    private static Account OpenBankAccount(
        Bank bank,
        string accountType,
        string customerId,
        decimal initialDeposit
    )
    {
        decimal paymentAmount = initialDeposit switch
        {
            <= 12000 => initialDeposit / 12,
            <= 24000 => initialDeposit / 24,
            <= 36000 => initialDeposit / 36,
            <= 48000 => initialDeposit / 48,
            _ => initialDeposit / 60
        };
        return accountType switch
            {
                "CHECKING" => bank.OpenCheckingAccount(customerId, initialDeposit),
                "LOAN" => bank.OpenLoanAccount(customerId, initialDeposit, paymentAmount),
                _ => throw new Exception($"Unknown Account Type: {accountType}")
            };
    }
    private static void OpenBanksAccounts(List<Bank> banks)
    {
        foreach (var bank in banks)
            foreach (var customer in bank.Customers)
            {
                string accountType = Helpers.PickOne(_accountTypes);
                decimal initialDeposit = accountType == "CHECKING"
                    ? _faker.Random.Decimal(12000, 120_000)
                    : _faker.Random.Decimal(6000, 60_000);
                OpenBankAccount(
                    bank: bank,
                    customerId: customer.Id,
                    accountType: accountType,
                    initialDeposit: initialDeposit
                );
            }
    }

    private static void DoBankOperations(List<Bank> banks)
    {
        string operation;
        decimal amount;
        foreach (var bank in banks)
        {
            var checkingAccounts = bank.Accounts.OfType<CheckingAccount>();
            foreach (var checkingAccount in checkingAccounts)
            {
                operation = Helpers.PickOne(_transactionTypes);
                switch(operation)
                {
                    case "DEPOSIT":
                        amount = _faker.Random.Decimal(2000, 20000);
                        Do(
                            $"Deposit {amount:C2} into account ({checkingAccount.AccountId})",
                            () => checkingAccount.Deposit(
                                amount,
                                description: $"Deposit of {amount:C2} at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
                            )
                        );
                        break;
                    case "WITHDRAWAL":
                        amount = _faker.Random.Decimal(1000, 5000);
                        Do(
                            $"Withdraw {amount:C2} from account ({checkingAccount.AccountId})",
                            () => checkingAccount.Withdraw(
                                amount,
                                description: $"Withdrawal of {amount:C2} at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
                            )
                        );
                        break;
                    case "TRANSFER":
                        var toCheckingAccount = Helpers.PickOne(checkingAccounts);
                        amount = _faker.Random.Decimal(1000, 5000);
                        if(
                            checkingAccount.Balance >= amount &&
                            checkingAccount.AccountId != toCheckingAccount.AccountId
                        )
                        {
                            Do(
                                $"Transfer {amount:C2} from account ({checkingAccount.AccountId}) to ({toCheckingAccount.AccountId})",
                                () => bank.Transfer(checkingAccount.AccountId, toCheckingAccount.AccountId, amount)
                            );
                        }
                        break;
                }
            }

            var loanAccounts = bank.Accounts.OfType<LoanAccount>();
            foreach (var loanAccount in loanAccounts)
            {
                amount = _faker.Random.Decimal(loanAccount.PaymentAmount, loanAccount.PaymentAmount * 2);
                Do(
                    $"Repay {amount:C2} from account ({loanAccount.AccountId})",
                    () => loanAccount.MakeRepayment(amount)
                );
            }
        }
    }

    private static void RunExceptionScenarios(List<Bank> banks)
    {
        Bank bank;
        CheckingAccount checkingAccount;
        LoanAccount loanAccount;
        Customer customer;
        decimal amount;

        // ── withdraw beyond limit ───────────────────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        checkingAccount = Helpers.PickOne(bank.Accounts.OfType<CheckingAccount>());
        customer = bank.GetCustomer(checkingAccount.CustomerId);
        amount = checkingAccount.Balance + checkingAccount.OverdraftLimit + 1000m;
        Do(
            $" Overdraft Beyond Limit | " +
            $"Trying To Withdraw ({amount:C2}) From Account ({checkingAccount.AccountId}) | " +
            $"Owned by ({customer.FullName}) That Leads To (insufficient funds) Exception",
            () => checkingAccount.Withdraw(
                amount,
                description: $"Withdrawal of {amount:C2} at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
            )
        );

        // ── deposit to frozen account ──────────────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        checkingAccount = Helpers.PickOne(bank.Accounts.OfType<CheckingAccount>());
        customer = bank.GetCustomer(checkingAccount.CustomerId);
        amount = 1000m;
        checkingAccount.Freeze();
        Do(
            $" Frozen Account | " +
            $"Trying To Deposit ({amount:C2}) To Account ({checkingAccount.AccountId}) | " +
            $"Owned by ({customer.FullName}) That Leads To (Account Frozen) Exception",
            () => checkingAccount.Deposit(
                amount,
                description: $"Deposit of {amount:C2} at {DateTime.Now:yyyy-MM-dd hh:mm tt}"
            )
        );
        checkingAccount.Activate();

        // ── repayment negative amount from a loan account ───────────────────────────────
        bank = Helpers.PickOne(banks);
        loanAccount = Helpers.PickOne(bank.Accounts.OfType<LoanAccount>());
        customer = bank.GetCustomer(loanAccount.CustomerId);
        amount = -3000m;
        Do(
            $" Loan Account | " +
            $"Trying To Withdraw ({amount:C2}) From Account ({loanAccount.AccountId}) | " +
            $"Owned by ({customer.FullName}) That Leads To (Invalid Amount) Exception",
            () => loanAccount.MakeRepayment(amount)
        );

        // ── insufficient repayment over a loan account ───────────────────────────────────
        bank = Helpers.PickOne(banks);
        loanAccount = Helpers.PickOne(bank.Accounts.OfType<LoanAccount>());
        customer = bank.GetCustomer(loanAccount.CustomerId);
        amount = loanAccount.MinPayment - 10m;
        Do(
            $" Loan Account | " +
            $"Trying To Repayment ({amount:C2}) Over Loan Account ({loanAccount.AccountId}) | " +
            $"Owned by ({customer.FullName}) That Leads To (Insufficient Payment) Exception",
            () => loanAccount.MakeRepayment(amount)
        );

        // ── get a customer that doesn't exist ────────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        Do(
            $" Customer That Doesn't Exist | " +
            $"Trying To Get Customer ({customer.Id}) That Leads To (Customer Not Found) Exception",
            () => bank.GetCustomer("CUSTOMER-012500460054")
        );

        // ── get an account that doesn't exist ────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        Do(
            $" Checking Account That Doesn't Exist | " +
            $"Trying To Get Checking Account ({checkingAccount.AccountId}) That Leads To (Checking Account Not Found) Exception",
            () => bank.GetAccount("CHECKING-012500460054")
        );

        // ── get a miss typed account ──────────────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        loanAccount = Helpers.PickOne(bank.Accounts.OfType<LoanAccount>());
        Do(
            $" Loan Account MissTyped As Checking Account | " +
            $"Trying To Get Checking Account ({loanAccount.AccountId}) That Leads To (Invalid Cast) Exception",
            () => bank.GetAccount<CheckingAccount>(loanAccount.AccountId)
        );

        // ── trying to close not empty account ────────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        do {
            checkingAccount = Helpers.PickOne(bank.Accounts.OfType<CheckingAccount>());
        } while (checkingAccount.Balance == 0);
        Do(
            $" Closing Checking Account With A Non-Zero Balance | " +
            $"Trying To Close Account ({checkingAccount.AccountId}) That Leads To (Account Not Empty) Exception",
            () => checkingAccount.Close()
        );

        // ── trying to transfer to same account ──────────────────────────────────────────
        bank = Helpers.PickOne(banks);
        checkingAccount = Helpers.PickOne(bank.Accounts.OfType<CheckingAccount>());
        amount = 1000m;
        Do(
            $" Trying To Transfer ({amount:C2}) From Account ({checkingAccount.AccountId}) To Itself That Leads To (Invalid Transfer) Exception",
            () => bank.Transfer(checkingAccount.AccountId, checkingAccount.AccountId, amount)
        );

    }

    private static async Task WriteBanksToJsonFilesAsync(List<Bank> banks)
    {
        if(!Directory.Exists(DATA_DIRECTORY))
            Directory.CreateDirectory(DATA_DIRECTORY);
        foreach (var bank in banks)
            await Helpers.WriteToJsonFileAsync(
                filePath: Path.Combine(DATA_DIRECTORY, $"{bank.FullName}.json"),
                data: bank
            );
    }

    public static async Task Run()
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // ── printing app header ────────────────────────────────────────────────────
            Helpers.PrintHeader("Start of Bank Management App");

            // ── creating 2 banks ───────────────────────────────────────────────────────
            Helpers.PrintSection("Creating 3 Banks");
            var cib = new Bank("CIB", "Commercial International Bank", "Cairo");
            var qnb = new Bank("QNB", "Qatar National Bank", "Cairo");
            var faisal = new Bank("FAISAL", "Faisal Islamic Bank of Egypt", "Cairo");
            List<Bank> banks = [cib, qnb, faisal];
            Helpers.PrintSuccess("3 Banks Created Successfully...");

            // ── registering some customers in each bank ─────────────────────────────────
            Helpers.PrintSection("Registering some Customers in each Bank");
            var cibCustomers = RegisterCustomers(cib, 10);
            var qnbCustomers = RegisterCustomers(qnb, 10);
            var faisalCustomers = RegisterCustomers(faisal, 10);
            Helpers.PrintSuccess("Customers Registered Successfully...");

            // ── printing some customers in each bank ────────────────────────────────────
            Helpers.PrintSection("Printing some customers in each Bank");
            foreach (var customer in cibCustomers.Shuffle().Take(3))
                Console.WriteLine(customer);
            foreach (var customer in qnbCustomers.Shuffle().Take(3))
                Console.WriteLine(customer);
            foreach (var customer in faisalCustomers.Shuffle().Take(3))
                Console.WriteLine(customer);
            Helpers.PrintSuccess("Customers Printed Successfully...");

            // ── creating some accounts for customers in each bank ───────────────────────
            Helpers.PrintSection("Creating some Accounts for Customers in each Bank");
            OpenBanksAccounts(banks);
            Helpers.PrintSuccess("Accounts Created Successfully...");

            // ── daily operations ────────────────────────────────────────────────────────
            Helpers.PrintSection("Daily Transactions");
            DoBankOperations(banks);
            Helpers.PrintSuccess("Daily Transactions Done Successfully...");

            // ── monthly processing (polymorphism) ───────────────────────────────────────
            Helpers.PrintSection("Monthly Processing");
            foreach(var bank in banks)
                bank.RunMonthlyProcessing();
            Helpers.PrintSuccess("Monthly Processing Done Successfully...");

            // ─── printing some accounts in each bank ────────────────────────────────────
            Helpers.PrintSection("Printing some accounts in each Bank");
            foreach (var account in cib.Accounts.Shuffle().Take(3))
                Console.WriteLine(account);
            foreach (var account in qnb.Accounts.Shuffle().Take(3))
                Console.WriteLine(account);
            foreach (var account in faisal.Accounts.Shuffle().Take(3))
                Console.WriteLine(account);
            Helpers.PrintSuccess("Accounts Printed Successfully...");

            // ─── printing some accounts statements in each bank ──────────────────────────
            Helpers.PrintSection("Printing some accounts statements in each Bank");
            foreach (var account in cib.Accounts.Shuffle().Take(3))
                Console.WriteLine(account.Statement);
            foreach (var account in qnb.Accounts.Shuffle().Take(3))
                Console.WriteLine(account.Statement);
            foreach (var account in faisal.Accounts.Shuffle().Take(3))
                Console.WriteLine(account.Statement);
            Helpers.PrintSuccess("Accounts Printed Successfully...");

            // ─── printing each bank portfolio summary ────────────────────────────────────
            Helpers.PrintSection("Printing Each Bank Portfolio Summary");
            foreach (var bank in banks)
                Console.WriteLine(bank.PortfolioSummary);
            Helpers.PrintSuccess("Each Bank Portfolio Summary Printed Successfully...");

            // ── run some exceptions scenarios ────────────────────────────────────────────
            Helpers.PrintSection("Running Exception Scenarios");
            RunExceptionScenarios(banks);
            Helpers.PrintSuccess("Running Exception Scenarios Done Successfully...");

            // ─── writing banks to json files ─────────────────────────────────────────────
            Helpers.PrintSection("Writing Banks to JSON Files");
            await WriteBanksToJsonFilesAsync(banks);
            Helpers.PrintSuccess("Writing Banks to JSON Files Done Successfully...");

            // ──── printing app footer ────────────────────────────────────────────────────
            Helpers.PrintFooter("End of Bank Management App");
        }
        catch (Exception ex) { Helpers.PrintError(ex.Message); }
    }
}
