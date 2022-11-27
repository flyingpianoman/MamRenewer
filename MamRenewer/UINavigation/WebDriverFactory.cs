using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.UINavigation
{
    class WebDriverFactory
    {
        private readonly IServiceProvider _sp;
        private readonly IConfiguration _configuration;

        public WebDriverFactory(IServiceProvider sp, IConfiguration configuration)
        {
            _sp = sp;
            _configuration = configuration;
        }

        public IWebDriver Create()
        {
            var proxyEnabled = _configuration.GetValue<bool>("Proxy:Enabled");
            var address = _configuration.GetValue<string>("Proxy:Address");

            var proxySettings = proxyEnabled
                ? new Proxy { HttpProxy = address, SslProxy = address, Kind = ProxyKind.Manual, IsAutoDetect = false }
                : null;

            var capabilities = new FirefoxOptions()
            {
                Proxy = proxySettings
            }.ToCapabilities();

            return new RemoteWebDriver(_configuration.GetValue<Uri>("MamBot:SeleniumHubAddress"), capabilities);
        }
    }
}
