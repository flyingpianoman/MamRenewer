using MamRenewer.Mam;
using MamRenewer.Services;
using MamRenewer.UINavigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Jobs
{
    class RenewMamVipJob : JobBase<RenewMamVipJob>
    {
        private readonly WebDriverFactory _webDriverFactory;
        private readonly MamBot _mamBot;

        public RenewMamVipJob(IHttpClientFactory httpClientFactory,
            MamBot mamBot,
            WebDriverFactory webDriverFactory,
            ILogger<RenewMamVipJob> logger,
            IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
            _webDriverFactory = webDriverFactory;
            _mamBot = mamBot;
        }

        public async Task ExecuteAsync()
        {
            await ConcurrencyLock.WaitAsync();
            var webDriver = _webDriverFactory.Create();
            try
            {
                await ExecuteCoreAsync(webDriver);
            }
            finally
            {
                webDriver.Quit();
                ConcurrencyLock.Release();
            }
        }

        private async Task ExecuteCoreAsync(IWebDriver webDriver)
        {
            _logger.LogInformation("Renewing MAM vip status");

            if (_proxyEnabled)
            {
                var currentExternalIP = GetCurrentIP(webDriver);
                await ValidateProxiedIPAsync(currentExternalIP);
            }

            await _mamBot.RenewVipStatusAsync(webDriver);

            _logger.LogInformation("MAM vip status renewed");
        }
    }
}
