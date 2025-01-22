using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;

namespace Betsson.OnlineWallets.UnitTests.Services
{
    public class OnlineWalletServiceUnitTests
    {
        private readonly Mock<IOnlineWalletRepository> _repositoryMock;
        private readonly OnlineWalletService _service;

        public OnlineWalletServiceUnitTests()
        {
            _repositoryMock = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_repositoryMock.Object);
        }

        // TC_01: Test GetBalanceAsync method when there are no transactions (balance is 0)
        [Fact]
        public async Task GetBalanceAsync_NoTransactions_ReturnsZeroBalance()
        {
            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                           .ReturnsAsync((OnlineWalletEntry?)null);

            var balance = await _service.GetBalanceAsync();

            // Assertion for Verifying GetBalanceAsync function returned zero when no transactions are present.
            Assert.StrictEqual(0, balance.Amount);
        }

        // TC_02: Test GetBalanceAsync method when there is a transaction (balance is calculated)
        [Fact]
        public async Task GetBalanceAsync_WithTransaction_ReturnsCorrectBalance()
        {
            var onlineWalletEntry = new OnlineWalletEntry
            {
                BalanceBefore = 100m,
                Amount = 50m
            };

            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                           .ReturnsAsync(onlineWalletEntry);

            var balance = await _service.GetBalanceAsync();

            // Assertion for Verifying GetBalanceAsync function returned correct balance when transactions are present.
            Assert.Equal(150, balance.Amount);
        }

        // TC_03: Test DepositFundsAsync method after deposited money. 
        [Fact]
        public async Task DepositFundsAsync_ValidDeposit_ReturnsUpdatedBalance()
        {
            var deposit = new Deposit { Amount = 100m };

            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                           .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 150m });

            _repositoryMock.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
                           .Returns(Task.CompletedTask);

            var balance = await _service.DepositFundsAsync(deposit);

            // Assertion for Verifying DepositFundsAsync function to return updated balance once after depositted. 
            Assert.Equal(250, balance.Amount);
        }

    }
}
