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
        private readonly LoginPage _loginPage;
        private readonly HomePage _homePage;
        private readonly StorePage _storePage;
        private readonly ScreenShotter _screenShotter;

        public MamBot(IWebDriver webDriver, 
            IConfiguration configuration,
            LoginPage loginPage,
            HomePage homePage,
            StorePage storePage,
            ScreenShotter screenShotter)
        {
            _webDriver = webDriver;
            _username = configuration.GetValue<string>("MamBot:Mam:Username");
            _password = configuration.GetValue<string>("MamBot:Mam:Password");
            _loginPage = loginPage;
            _homePage = homePage;
            _storePage = storePage;
            _screenShotter = screenShotter;
        }

        public async Task RefreshIPAsync()
        {
            LoadFreshMamUrl();

            await LoginAsync();
        }

        public async Task RenewVipStatusAsync()
        {
            LoadFreshMamUrl();

            await LoginAsync();

            await NavigateToBonusPointsAsync();

            await PurchaseMaxVipDutationAsync();

            _screenShotter.TakeScreenshot("vipPurchase");
        }

        private void LoadFreshMamUrl()
        {
            _webDriver.Manage().Cookies.DeleteAllCookies();
            _webDriver.Url = _mamUrl;
            _webDriver.Navigate();
        }

        private async Task LoginAsync()
        {
            await _loginPage.InitializeAsync(_webDriver);
            _loginPage.SetUserName(_username);
            _loginPage.SetPassword(_password);

            _loginPage.ClickLogin();
        }

        private async Task NavigateToBonusPointsAsync()
        {
            await _homePage.InitializeAsync(_webDriver);
            _homePage.NavigateToBonusPoints();
        }

        private async Task PurchaseMaxVipDutationAsync()
        {
            await _storePage.InitializeAsync(_webDriver);
            _storePage.ToggleVipSection();
            _storePage.PurchaseMaxVipDuration();
        }
    }
}
