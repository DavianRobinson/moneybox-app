using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
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
            fromAccount.Credit(1000m);

            var toAccount = new Account(toUser);
            toAccount.Credit(1000m);



            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();


            var sut = new TransferMoney(accountRepo.Object, notificationService.Object,
                transferMoneyRepository.Object);


            //act
            sut.Execute(fromAccount.Id, toAccount.Id, 10m);

            //assert
            Assert.Equal(990m, fromAccount.Balance);
            Assert.Equal(1010m, toAccount.Balance);
        }

        [Fact]
        public void TransferMoneyInsufficientFunds()
        {
            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(5m);



            var toAccount = new Account(toUser);
            toAccount.Credit(1000m);


            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);





            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(fromAccount.Id, toAccount.Id, 10m));
            Assert.Equal(5m, fromAccount.Balance);
            Assert.Equal(1000m, toAccount.Balance);
        }

        [Fact]
        public void TransferMoneyFundsLowLimit()
        {
            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(400m);

            var toAccount = new Account(toUser);
            toAccount.Credit(1000m);


            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);


            //act
            sut.Execute(fromAccount.Id, toAccount.Id, 10m);


            //assert
            notificationService.Verify(x => x.NotifyFundsLow(fromUser.Email));
            Assert.Equal(390m, fromAccount.Balance);
            Assert.Equal(1010m, toAccount.Balance);
        }
        [Fact]
        public void TransferMoneyOverPaidinLimit()
        {
            //arrange

            var fromAccount = new Account(fromUser);
            fromAccount.Credit(1000m);

            var toAccount = new Account(toUser);
            toAccount.Credit(3995m);


            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);




            //assert
            Assert.Throws<InvalidOperationException>(() => sut.Execute(fromAccount.Id, toAccount.Id, 10m));
            Assert.Equal(1000m, fromAccount.Balance);
            Assert.Equal(3995m, toAccount.Balance);

        }

        [Fact]
        public void TransferMoneyNearPaidinLimit()
        {
            //arrange


            var fromAccount = new Account(fromUser);
            fromAccount.Credit(400m);


            var toAccount = new Account(toUser);
            toAccount.Credit(3600m);

            var accountRepo = new Mock<IAccountRepository>();
            accountRepo.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            accountRepo.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            var notificationService = new Mock<INotificationService>();

            var sut = new TransferMoney(accountRepo.Object, notificationService.Object, transferMoneyRepository.Object);


            //act
            sut.Execute(fromAccount.Id, toAccount.Id, 10m);


            //assert
            notificationService.Verify(x => x.NotifyApproachingPayInLimit(toUser.Email));
            Assert.Equal(390m, fromAccount.Balance);
            Assert.Equal(3610m, toAccount.Balance);

        }
    }
}
