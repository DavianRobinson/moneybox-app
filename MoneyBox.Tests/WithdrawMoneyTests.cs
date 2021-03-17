using System;
using Xunit;
using Moneybox.App.Domain;
using Moneybox.App.Features;
using Moneybox.App.DataAccess;
using Moq;
using Moneybox.App.Domain.Services;

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
            TestAccount.Credit(1000m);

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);


            //act
            sut.Execute(TestAccount.Id, 10m);

            //assert
            Assert.Equal(990m, TestAccount.Balance);
        }
        [Fact]
        public void WithdrawMoneyZeroBalance()
        {
            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(0m);

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);
            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);
            //act
            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(TestAccount.Id, 10m));

        }
        [Fact]
        public void WithdrawMoneyInsufficientFunds()
        {
            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(5m);

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);
            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);

            //act
            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(TestAccount.Id, 10m));
        }

        [Fact]
        public void WithdrawMoneyFundsLowLimit()
        {
            //arrange

            var TestAccount = new Account(user);
            TestAccount.Credit(500m);

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(TestAccount.Id)).Returns(TestAccount);
            var notificationService = new Mock<INotificationService>();

            var sut = new WithdrawMoney(accountRepo.Object, notificationService.Object);

            //act
            sut.Execute(TestAccount.Id, 10m);

            //assert
            notificationService.Verify(x => x.NotifyFundsLow(user.Email));

        }
    }
}
