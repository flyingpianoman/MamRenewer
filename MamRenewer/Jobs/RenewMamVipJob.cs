using MamRenewer.Mam;
using MamRenewer.Services;
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
        private readonly IWebDriver _webDriver;
        private readonly MamBot _mamBot;

        public RenewMamVipJob(IHttpClientFactory httpClientFactory,
            MamBot mamBot,
            IWebDriver webDriver,
            ILogger<RenewMamVipJob> logger,
            IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
            _webDriver = webDriver;
            _mamBot = mamBot;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                await ExecuteCoreAsync();
            }
            finally
            {
                _webDriver.Quit();
            }
        }

        private async Task ExecuteCoreAsync()
        {
            _logger.LogInformation("Renewing MAM vip status");

            if (_proxyEnabled)
            {
                var currentExternalIP = GetCurrentIP(_webDriver);
                await ValidateProxiedIPAsync(currentExternalIP);
            }

            await _mamBot.RenewVipStatusAsync();

            _logger.LogInformation("MAM vip status renewed");
        }
    }
}
