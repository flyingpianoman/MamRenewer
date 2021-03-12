using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MamRenewer.Jobs
{
    internal class JobBase<T>
    {
        protected readonly bool _proxyEnabled;
        protected IHttpClientFactory _httpClientFactory;
        protected ILogger<T> _logger;

        public JobBase(IHttpClientFactory httpClientFactory, ILogger<T> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _proxyEnabled = configuration.GetValue<bool>("Proxy:Enabled");
        }

        protected static async Task<string> GetCurrentIPAsync(HttpClient client)
        {
            return (await client.GetStringAsync("http://ipinfo.io/ip")).Trim();
        }

        protected async Task ValidateProxiedIPAsync(string currentExternalIP)
        {
            var unproxiedClient = _httpClientFactory.CreateClient();
            var currentExternalUnproxiedIP = await GetCurrentIPAsync(unproxiedClient);

            if (currentExternalUnproxiedIP == currentExternalIP)
            {
                _logger.LogWarning("Current external IP is the same as an unproxied IP '{0}', failing", currentExternalIP);
                throw new InvalidOperationException("Current external IP is the same as an unproxied IP, stopping");
            }
        }
    }
}