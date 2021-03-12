using MamRenewer.Mam;
using MamRenewer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Jobs
{
    class RenewMamVipJob : JobBase<SignInToMamJob>
    {
        private PreviousJobInfoRepository _previousJobInfoRepository;
        private readonly MamBot _mamBot;

        public RenewMamVipJob(PreviousJobInfoRepository previousJobInfoRepository,
            IHttpClientFactory httpClientFactory,
            MamBot mamBot,
            ILogger<SignInToMamJob> logger,
            IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
            _previousJobInfoRepository = previousJobInfoRepository;
            _mamBot = mamBot;
        }

        public async Task ExecuteAsync()
        {
            var client = _httpClientFactory.CreateClient(Program.ProxiedHttpClientName);
            var currentExternalIP = await GetCurrentIPAsync(client);

            if (_proxyEnabled)
            {
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
