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
    class RefreshMamIPJob : JobBase<RenewMamVipJob>
    {
        private readonly IWebDriver _webDriver;
        private PreviousJobInfoRepository _previousJobInfoRepository;
        private readonly MamBot _mamBot;

        public RefreshMamIPJob(PreviousJobInfoRepository previousJobInfoRepository,
            IHttpClientFactory httpClientFactory,
            IWebDriver webDriver,
            MamBot mamBot,
            ILogger<RefreshMamIPJob> logger,
            IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
            _webDriver = webDriver;
            _previousJobInfoRepository = previousJobInfoRepository;
            _mamBot = mamBot;
        }

        public async Task ExecuteAsync()
        {
            var currentExternalIP = await GetCurrentIPAsync(_webDriver);

            if (_proxyEnabled)
            {
                _logger.LogDebug("Proxy is enabled, validating if IP is actually different");
                await ValidateProxiedIPAsync(currentExternalIP);
            }

            var lastUsedIp = _previousJobInfoRepository.RetrieveLastUsedIP();
            _logger.LogDebug("Current external IP is '{0}'", currentExternalIP);

            if (currentExternalIP == lastUsedIp)
            {
                _logger.LogDebug("Current external IP is equal to the last used ip, skipping login");
                return;
            }

            _logger.LogInformation("Current external IP '{0}' differs from last used IP {1}. Logging in to MAM to update IP",
                currentExternalIP, lastUsedIp);

            await _mamBot.RefreshIPAsync();

            _logger.LogInformation("IP refreshed");

            _previousJobInfoRepository.UpdateLastUsedIP(currentExternalIP);
        }
    }
}
