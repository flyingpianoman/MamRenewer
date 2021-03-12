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
    class SignInToMamJob : JobBase<SignInToMamJob>
    {
        private readonly MamBot _mamBot;

        public SignInToMamJob(IHttpClientFactory httpClientFactory,
            MamBot mamBot,
            ILogger<SignInToMamJob> logger,
            IConfiguration configuration)
            : base(httpClientFactory, logger, configuration)
        {
            _mamBot = mamBot;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Renewing MAM vip status");
            var client = _httpClientFactory.CreateClient(Program.ProxiedHttpClientName);

            if (_proxyEnabled)
            {
                var currentExternalIP = await GetCurrentIPAsync(client);
                await ValidateProxiedIPAsync(currentExternalIP);
            }

            await _mamBot.RenewVipStatusAsync();

            _logger.LogInformation("MAM vip status renewed");

            _previousJobInfoRepository.UpdateLastUsedIP(currentExternalIP);
        }
    }
}
