using MamRenewer.Polly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MamRenewer.Jobs
{
    internal class JobBase<T>
    {
        private const string _ExternalIPCheckUrl = "https://ipinfo.io/ip";
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
            return (await client.GetStringAsync(_ExternalIPCheckUrl)).Trim();
        }
        protected static async Task<string> GetCurrentIPAsync(IWebDriver webDriver)
        {
            webDriver.Url = _ExternalIPCheckUrl;
            webDriver.Navigate();

            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var bodyEls = webDriver.FindElements(By.CssSelector("body"));
                    var body = bodyEls.FirstOrDefault()?.Text?.Trim();
                    if (body != null && !Regex.IsMatch(body, @"^\d+\.\d+\.\d+\.\d+$"))
                    {
                        throw new PageHelper.RetryException();
                    }
                });

            return webDriver.FindElement(By.CssSelector("body")).Text.Trim();
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