using MamRenewer.Mam.Pages;
using MamRenewer.UINavigation;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MamRenewer.Mam
{
    class MamBot
    {
        private const string _mamUrl = @"https://www.myanonamouse.net";
        private readonly IWebDriver _webDriver;
        private readonly string _username;
        private readonly string _password;
        private readonly ScreenShotter _screenShotter;

        public MamBot(IWebDriver webDriver, 
            IConfiguration configuration)
        {
            _webDriver = webDriver;
            _username = configuration.GetValue<string>("MamBot:Mam:Username");
            _password = configuration.GetValue<string>("MamBot:Mam:Password");
            _screenShotter = new ScreenShotter(_webDriver, configuration);
        }

        public async Task RefreshIPAsync()
        {
            LoadMam();

            await LoginAsync();
        }

        public async Task RenewVipStatusAsync()
        {
            LoadMam();

            await LoginAsync();

            await NavigateToBonusPointsAsync();

            await PurchaseMaxVipDutationAsync();

            _screenShotter.TakeScreenshot("vipPurchase");
        }

        private void LoadMam()
        {
            _webDriver.Url = _mamUrl;
            _webDriver.Navigate();
        }

        private async Task LoginAsync()
        {
            var loginPage = new LoginPage();
            await loginPage.InitializeAsync(_webDriver);
            loginPage.SetUserName(_username);
            loginPage.SetPassword(_password);

            loginPage.ClickLogin();
        }

        private async Task NavigateToBonusPointsAsync()
        {
            var homePage = new HomePage();
            await homePage.InitializeAsync(_webDriver);
            homePage.NavigateToBonusPoints();
        }

        private async Task PurchaseMaxVipDutationAsync()
        {
            var storePage = new StorePage();
            await storePage.InitializeAsync(_webDriver);
            storePage.ToggleVipSection();
            storePage.PurchaseMaxVipDuration();
        }
    }
}
