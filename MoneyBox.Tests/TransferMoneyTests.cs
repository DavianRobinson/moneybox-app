using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NodaMoney;
using Xunit;

namespace Moneybox.Tests
{
    public class TransferMoneyTests
    {
        private readonly User fromUser = new User(Guid.NewGuid(), "fromAccount@email.com", "Test From");
        private readonly User toUser = new User(Guid.NewGuid(), "toAccount@email.com", "Test To");
        Mock<ITransferMoneyRepository> transferMoneyRepository = null;
        public TransferMoneyTests()
        {
            transferMoneyRepository = new Mock<ITransferMoneyRepository>();
        }

        [Fact]
        public void TransferMoney()
        {

            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var toAccount = new Account(toUser);
            toAccount.Credit(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)));



            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();


            var sut = new TransferMoney(accountRepo.Object, notificationService.Object,
                transferMoneyRepository.Object);


            //act
            sut.Execute(fromAccount.Id, toAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode)));

            //assert
            Assert.Equal(new Money(990m, Currency.FromCode(Account.DefaultCurrencyCode)), fromAccount.Balance);
            Assert.Equal(new Money(1010m, Currency.FromCode(Account.DefaultCurrencyCode)), toAccount.Balance);
        }

        [Fact]
        public void TransferMoneyInsufficientFunds()
        {
            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(new Money(5m, Currency.FromCode(Account.DefaultCurrencyCode)));



            var toAccount = new Account(toUser);
            toAccount.Credit(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)));


            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);





            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(fromAccount.Id, toAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode))));
            Assert.Equal(new Money(5m, Currency.FromCode(Account.DefaultCurrencyCode)), fromAccount.Balance);
            Assert.Equal(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)), toAccount.Balance);
        }

        [Fact]
        public void TransferMoneyFundsLowLimit()
        {
            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(new Money(400m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var toAccount = new Account(toUser);
            toAccount.Credit(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)));


            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);


            //act
            sut.Execute(fromAccount.Id, toAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode)));


            //assert
            notificationService.Verify(x => x.NotifyFundsLow(fromUser.Email));
            Assert.Equal(new Money(390m, Currency.FromCode(Account.DefaultCurrencyCode)), fromAccount.Balance);
            Assert.Equal(new Money(1010m, Currency.FromCode(Account.DefaultCurrencyCode)), toAccount.Balance);
        }
        [Fact]
        public void TransferMoneyOverPaidinLimit()
        {
            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var toAccount = new Account(toUser);
            toAccount.Credit(new Money(3995m, Currency.FromCode(Account.DefaultCurrencyCode)));


            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);




            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(fromAccount.Id, toAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode))));
            Assert.Equal(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)), fromAccount.Balance);
            Assert.Equal(new Money(3995m, Currency.FromCode(Account.DefaultCurrencyCode)), toAccount.Balance);

        }

        [Fact]
        public void TransferMoneyNearPaidinLimit()
        {
            //arrange


            var fromAccount = new Account(fromUser);
            fromAccount.Credit(new Money(400m, Currency.FromCode(Account.DefaultCurrencyCode)));


            var toAccount = new Account(toUser);
            toAccount.Credit(new Money(3600m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);


            //act
            sut.Execute(fromAccount.Id, toAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode)));


            //assert
            notificationService.Verify(x => x.NotifyApproachingPayInLimit(toUser.Email));
            Assert.Equal(new Money(390m, Currency.FromCode(Account.DefaultCurrencyCode)), fromAccount.Balance);
            Assert.Equal(new Money(3610m, Currency.FromCode(Account.DefaultCurrencyCode)), toAccount.Balance);

        }
    }
}
