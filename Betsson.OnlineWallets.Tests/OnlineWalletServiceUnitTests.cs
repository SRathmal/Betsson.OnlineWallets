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

        // TC_04: Test DepositFundsAsync method when deposited invalid amount.
        [Fact]
        public async Task DepositFundsAsync_InvalidDeposit_ReturnsException()
        {
            var deposit = new Deposit { Amount = -5m };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.DepositFundsAsync(deposit));

            //Assertion for Verifying ArugmentException throwed correctly. 
            Assert.Equal("Deposit amount should be valid", exception.Message);

        }

        // TC_05: Test WithdrawFundsAsync method when there is sufficient balance.
        [Fact]
        public async Task WithdrawFundsAsync_ValidWithdrawal_ReturnsUpdatedBalance()
        {
            var withdrawal = new Withdrawal { Amount = 50m };

            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                           .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 100m });

            _repositoryMock.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
                           .Returns(Task.CompletedTask);

            var balance = await _service.WithdrawFundsAsync(withdrawal);

            // Assertion for verifying WithdrawFundsAsync function to return updated balance once after withdrowed. 
            Assert.Equal(50, balance.Amount);
        }

        // TC_06: Test WithdrawFundsAsync when there is insufficient balance
        [Fact]
        public async Task WithdrawFundsAsync_InsufficientBalance_ThrowsInsufficientBalanceException()
        {
            var withdrawal = new Withdrawal { Amount = 150m };

            _repositoryMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
                           .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = 100m });

            // Assertion for verifying Insufficient Exception throwed correctly.  
            await Assert.ThrowsAsync<InsufficientBalanceException>(() => _service.WithdrawFundsAsync(withdrawal));
        }

    }
}
