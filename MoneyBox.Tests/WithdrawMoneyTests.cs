using System;
using Xunit;
using Moneybox.App.Domain;
using Moneybox.App.Features;
using Moneybox.App.DataAccess;
using Moq;
using Moneybox.App.Domain.Services;
using NodaMoney;

namespace Moneybox.Tests
{
    public class WithdrawMoneyTests
    {
        private readonly User user = new User(Guid.NewGuid(), "testing@email.com", "Test");

        public WithdrawMoneyTests()
        {

        }
        [Fact]
        public void WithdrawMoney()
        {

            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(new Money(1000m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);


            //act
            sut.Execute(TestAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode)));

            //assert
            Assert.Equal(new Money(990m, Currency.FromCode(Account.DefaultCurrencyCode)), TestAccount.Balance);
        }
        [Fact]
        public void WithdrawMoneyZeroBalance()
        {
            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(new Money(0m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);
            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);
            //act
            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(TestAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode))));

        }
        [Fact]
        public void WithdrawMoneyInsufficientFunds()
        {
            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(new Money(5m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);
            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);

            //act
            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(TestAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode))));
        }

        [Fact]
        public void WithdrawMoneyFundsLowLimit()
        {
            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(new Money(500m, Currency.FromCode(Account.DefaultCurrencyCode)));

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);
            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);

            //act
            sut.Execute(TestAccount.Id, new Money(10m, Currency.FromCode(Account.DefaultCurrencyCode)));

            //assert
            notificationService.Verify(x => x.NotifyFundsLow(user.Email));

        }
    }
}
