using System;
using Moneybox.App.Domain;

namespace Moneybox.App.DataAccess
{
    public interface ITransferMoneyRepository
    {
        void TransferMoney(Account fromAccount, Account toAccount);
    }
}