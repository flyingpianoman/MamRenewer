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
    class SignInToMamJob
    {
        private PreviousJobInfoRepository _previousJobInfoRepository;
        private IHttpClientFactory _httpClientFactory;
        private readonly MamBot _mamBot;
        private ILogger<SignInToMamJob> _logger;
        private readonly bool _proxyEnabled;

        public SignInToMamJob(PreviousJobInfoRepository previousJobInfoRepository,
            IHttpClientFactory httpClientFactory,
            MamBot mamBot,
            ILogger<SignInToMamJob> logger,
            IConfiguration configuration)
        {
            _previousJobInfoRepository = previousJobInfoRepository;
            _httpClientFactory = httpClientFactory;
            _mamBot = mamBot;
            _logger = logger;
            _proxyEnabled = configuration.GetValue<bool>("Proxy:Enabled");
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

            _previousJobInfoRepository.UpdateLastUsedIP(currentExternalIP);
        }

        private async Task ValidateProxiedIPAsync(string currentExternalIP)
        {
            var unproxiedClient = _httpClientFactory.CreateClient();
            var currentExternalUnproxiedIP = await GetCurrentIPAsync(unproxiedClient);

            if (currentExternalUnproxiedIP == currentExternalIP)
            {
                _logger.LogWarning("Current external IP is the same as an unproxied IP '{0}', failing", currentExternalIP);
                throw new InvalidOperationException("Current external IP is the same as an unproxied IP, stopping");
            }
        }

        private static async Task<string> GetCurrentIPAsync(HttpClient client)
        {
            return (await client.GetStringAsync("http://ipinfo.io/ip")).Trim();
        }
    }
}
