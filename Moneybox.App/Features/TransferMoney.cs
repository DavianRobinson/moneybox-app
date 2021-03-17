using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using NodaMoney;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;
        private ITransferMoneyRepository transferMoneyRepository;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService, ITransferMoneyRepository transferRepository)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
            this.transferMoneyRepository = transferRepository;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, Money amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);

            var fromBalance = from.Balance - amount;
            if (fromBalance < new Money(0m, Currency.FromCode(Account.DefaultCurrencyCode)))
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if (fromBalance < Account.FundsLowLimit)
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }

            var paidIn = to.PaidIn + amount;
            if (paidIn > Account.PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (Account.PayInLimit - paidIn < new Money(500m, Currency.FromCode(Account.DefaultCurrencyCode)))
            {
                this.notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }

            from.Debit(amount);

            to.Credit(amount);

            // transfer money as one transaction DB
            this.transferMoneyRepository.TransferMoney(from, to);

        }
    }
}
