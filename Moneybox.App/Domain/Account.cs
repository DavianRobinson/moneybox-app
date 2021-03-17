using System;
using NodaMoney;

namespace Moneybox.App.Domain
{
    public class Account
    {
        private static readonly Money payInLimit = new Money(4000m, Currency.FromCode("GBP"));
        private static readonly Money fundsLowLimit = new Money(500m, Currency.FromCode("GBP"));
        private static readonly string defaultCurrencyCode = "GBP";
        private readonly Guid id;
        private Money balance;
        private Money withdrawn;
        private Money paidIn;
        private User user;



        public Account(User accountUser)
        {
            id = Guid.NewGuid();
            user = accountUser;
            balance = new Money(0m, Currency.FromCode("GBP"));
            paidIn = new Money(0m, Currency.FromCode("GBP"));
            withdrawn = new Money(0m, Currency.FromCode("GBP"));
        }

        public Account(Guid accountId, User accountUser)
        {
            id = accountId;
            user = accountUser;
            balance = new Money(0m, Currency.FromCode("GBP"));
            paidIn = new Money(0m, Currency.FromCode("GBP"));
            withdrawn = new Money(0m, Currency.FromCode("GBP"));
        }



        public Guid Id => id;
        public User User { get => user; }

        public Money Balance { get => balance; }

        public Money Withdrawn { get => withdrawn; }

        public Money PaidIn { get => paidIn; }

        public static Money FundsLowLimit => fundsLowLimit;

        public static Money PayInLimit => payInLimit;

        public static string DefaultCurrencyCode => defaultCurrencyCode;

        public void Debit(Money amount)
        {
            balance = balance - amount;
            withdrawn = withdrawn - amount;
        }
        public void Credit(Money amount)
        {
            balance = balance + amount;
            paidIn = paidIn + amount;
        }
    }
}
