using System;
using System.Collections.Generic;

namespace FinanceManagementSystem
{
    // ====== RECORD FOR TRANSACTION ======
    public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

    // ====== INTERFACE FOR PROCESSORS ======
    public interface ITransactionProcessor
    {
        void Process(Transaction transaction);
    }

    // ====== PROCESSOR IMPLEMENTATIONS ======
    public class BankTransferProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[Bank Transfer] Processed {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class MobileMoneyProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[Mobile Money] Processed {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class CryptoWalletProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[Crypto Wallet] Processed {transaction.Amount:C} for {transaction.Category}");
        }
    }

    // ====== BASE ACCOUNT CLASS ======
    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        public Account(string accountNumber, decimal initialBalance)
        {
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void ApplyTransaction(Transaction transaction)
        {
            Balance -= transaction.Amount;
        }
    }

    // ====== SEALED SAVINGS ACCOUNT ======
    public sealed class SavingsAccount : Account
    {
        public SavingsAccount(string accountNumber, decimal initialBalance)
            : base(accountNumber, initialBalance)
        {
        }

        public override void ApplyTransaction(Transaction transaction)
        {
            if (transaction.Amount > Balance)
            {
                Console.WriteLine("Insufficient funds");
            }
            else
            {
                Balance -= transaction.Amount;
                Console.WriteLine($"Transaction applied. New balance: {Balance:C}");
            }
        }
    }

    // ====== FINANCE APP ======
    public class FinanceApp
    {
        private List<Transaction> _transactions = new();

        public void Run()
        {
            // Create account
            SavingsAccount account = new SavingsAccount("1234567890", 1000m);

            // Create transactions
            Transaction t1 = new(1, DateTime.Now, 150m, "Groceries");
            Transaction t2 = new(2, DateTime.Now, 300m, "Utilities");
            Transaction t3 = new(3, DateTime.Now, 1200m, "Entertainment");

            // Processors
            ITransactionProcessor mobileMoney = new MobileMoneyProcessor();
            ITransactionProcessor bankTransfer = new BankTransferProcessor();
            ITransactionProcessor cryptoWallet = new CryptoWalletProcessor();

            // Process and apply transactions
            mobileMoney.Process(t1);
            account.ApplyTransaction(t1);
            _transactions.Add(t1);

            bankTransfer.Process(t2);
            account.ApplyTransaction(t2);
            _transactions.Add(t2);

            cryptoWallet.Process(t3);
            account.ApplyTransaction(t3);
            _transactions.Add(t3);
        }
    }

    // ====== MAIN ENTRY POINT ======
    public class Program
    {
        public static void Main(string[] args)
        {
            FinanceApp app = new FinanceApp();
            app.Run();
        }
    }
}