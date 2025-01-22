using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Betsson.OnlineWallets.Web.Models;
using Newtonsoft.Json;
using Xunit;

namespace Betsson.OnlineWallets.Tests
{
    public class ApiTests
    {
        private readonly HttpClient _httpClient;

        public ApiTests()
        {
            
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
        }

        public async Task InitializeAsync()
        {
            var resetResponse = await _httpClient.PostAsync("/api/resetbalance", null);
            resetResponse.EnsureSuccessStatusCode();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        //TC 01: Testcase for Balance API Request 
        [Fact]
        public async Task GetBalance_ShouldReturnBalance()
        {
            var response = await _httpClient.GetAsync("/onlinewallet/balance");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var balanceResponse = JsonConvert.DeserializeObject<BalanceResponse>(responseContent);

            Assert.NotNull(balanceResponse);
            Assert.True(balanceResponse.Amount >= 0);
        }

        //TC 02: Testcase for Withdrawal API Request
        [Fact]
        public async Task PostWithdrawal_ShouldProcessWithdrawal()
        {
            
            var balanceResponse = await _httpClient.GetAsync("/onlinewallet/balance");
            balanceResponse.EnsureSuccessStatusCode();

            var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
            var balanceJson = JsonConvert.DeserializeObject<BalanceResponse>(balanceContent);
            decimal currentBalance = balanceJson.Amount;

            Console.WriteLine("Current Balance: " + currentBalance);

            decimal withdrawalAmount = 50.0m; 
            var withdrawalRequest = new WithdrawalRequest { Amount = withdrawalAmount };
            var content = new StringContent(JsonConvert.SerializeObject(withdrawalRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/onlinewallet/withdraw", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);

            Console.WriteLine("Response Content: " + responseContent);

            decimal expectedBalance = currentBalance - withdrawalAmount;

            Assert.Equal(expectedBalance, responseJson.amount.ToObject<decimal>());
        }


        //TC 03: Testcase for Deposit API Request
        [Fact]
        public async Task PostDeposit_ShouldProcessDeposit()
        {
            var balanceResponse = await _httpClient.GetAsync("/onlinewallet/balance");
            balanceResponse.EnsureSuccessStatusCode();

            var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
            var balanceJson = JsonConvert.DeserializeObject<BalanceResponse>(balanceContent);
            decimal currentBalance = balanceJson.Amount;

            Console.WriteLine("Current Balance: " + currentBalance);

            decimal depositAmount = 250.0m; 
            var depositRequest = new DepositRequest { Amount = depositAmount };
            var content = new StringContent(JsonConvert.SerializeObject(depositRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/onlinewallet/deposit", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);

            Console.WriteLine("Response Content: " + responseContent);

            decimal expectedBalance = currentBalance + depositAmount;

            Assert.Equal(expectedBalance, responseJson.amount.ToObject<decimal>());
        }


        //TC 04: Test PostWithdrawal method when withdrawed invalid withdrawal amount 
        [Fact]
        public async Task PostWithdrawal_InvalidAmount_ShouldReturnBadRequest()
        {
            var withdrawalRequest = new WithdrawalRequest { Amount = -50.0m }; 
            var content = new StringContent(JsonConvert.SerializeObject(withdrawalRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/onlinewallet/withdraw", content);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        //TC 05: Test PostDeposit method when deposited invalid amount 
        [Fact]
        public async Task PostDeposit_InvalidAmount_ShouldReturnBadRequest()
        {
           
            var depositRequest = new DepositRequest { Amount = -100.0m }; 
            var content = new StringContent(JsonConvert.SerializeObject(depositRequest), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/onlinewallet/deposit", content);

            // Assert response is bad request
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
}
