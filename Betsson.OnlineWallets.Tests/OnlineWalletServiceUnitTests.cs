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

        
    }
}
