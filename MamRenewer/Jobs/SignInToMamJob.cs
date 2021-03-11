using MamRenewer.Services;
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
        private ILogger<SignInToMamJob> _logger;

        public SignInToMamJob(PreviousJobInfoRepository previousJobInfoRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<SignInToMamJob> logger)
        {
            _previousJobInfoRepository = previousJobInfoRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var client = _httpClientFactory.CreateClient(Program.ProxiedHttpClientName);
            var currentExternalIP = (await client.GetStringAsync("http://ipinfo.io/ip")).Trim();
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
    }
}
