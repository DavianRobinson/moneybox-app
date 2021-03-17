using System;

namespace Moneybox.App.Domain
{
    public class Account
    {
        private const decimal payInLimit = 4000m;
        private const decimal fundsLowLimit = 500m;
        private readonly Guid id;
        private decimal balance;
        private decimal withdrawn;
        private decimal paidIn;
        private User user;



        public Account(User accountUser)
        {
            id = Guid.NewGuid();
            user = accountUser;
            balance = 0m;
            paidIn = 0m;
            withdrawn = 0m;
        }

        public Account(Guid accountId, User accountUser)
        {
            id = accountId;
            user = accountUser;
            balance = 0m;
            paidIn = 0m;
            withdrawn = 0m;
        }



        public Guid Id => id;
        public User User { get => user; }

        public decimal Balance { get => balance; }

        public decimal Withdrawn { get => withdrawn; }

        public decimal PaidIn { get => paidIn; }

        public static decimal FundsLowLimit => fundsLowLimit;

        public static decimal PayInLimit => payInLimit;

        public void Debit(decimal amount)
        {
            balance = balance - amount;
            withdrawn = withdrawn - amount;
        }
        public void Credit(decimal amount)
        {
            balance = balance + amount;
            paidIn = paidIn + amount;
        }
    }
}
