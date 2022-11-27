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
        private readonly string _username;
        private readonly string _password;

        public MamBot(IConfiguration configuration)
        {
            _username = configuration.GetValue<string>("MamBot:Mam:Username");
            _password = configuration.GetValue<string>("MamBot:Mam:Password");
        }

        public async Task RefreshIPAsync(IWebDriver webDriver)
        {
            LoadMam(webDriver);

            await LoginAsync(webDriver);
        }

        public async Task RenewVipStatusAsync(IWebDriver webDriver)
        {
            LoadMam(webDriver);

            await LoginAsync(webDriver);

            await NavigateToBonusPointsAsync(webDriver);

            await PurchaseMaxVipDutationAsync(webDriver);
        }

        private void LoadMam(IWebDriver webDriver)
        {
            webDriver.Url = _mamUrl;
            webDriver.Navigate();
        }

        private async Task LoginAsync(IWebDriver webDriver)
        {
            var loginPage = new LoginPage();
            await loginPage.InitializeAsync(webDriver);
            loginPage.SetUserName(_username);
            loginPage.SetPassword(_password);

            loginPage.ClickLogin();
        }

        private async Task NavigateToBonusPointsAsync(IWebDriver webDriver)
        {
            var homePage = new HomePage();
            await homePage.InitializeAsync(webDriver);
            homePage.NavigateToBonusPoints();
        }

        private async Task PurchaseMaxVipDutationAsync(IWebDriver webDriver)
        {
            var storePage = new StorePage();
            await storePage.InitializeAsync(webDriver);
            storePage.ToggleVipSection();
            storePage.PurchaseMaxVipDuration();
        }
    }
}
