using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {

            var from = this.accountRepository.GetAccountById(fromAccountId);

            if (from.Balance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make withdrawal");
            }

            var fromBalance = from.Balance - amount;

            if (fromBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make withdrawal");
            }

            if (fromBalance < Account.FundsLowLimit)
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }

            from.Debit(amount);

            this.accountRepository.Update(from);
        }
    }
}
