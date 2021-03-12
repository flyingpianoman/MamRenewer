using MamRenewer.Polly;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Mam.Pages
{
    class HomePage
    {
        private const string _lastTorrentRowsSelector = "#fpTor #search > tbody > tr";
        private const string _bonusPointsLinkSelector = "a#tmBP";
        private IWebDriver _webDriver;

        public HomePage()
        {
        }

        public ValueTask InitializeAsync(IWebDriver webDriver)
        {
            _webDriver = webDriver;

            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    //We check the lastTorrentsRow since that's loaded after the initial page load
                    //This ensures that at least the dropdown menu's are loaded
                    var elements = _webDriver.FindElements(By.CssSelector(_lastTorrentRowsSelector));
                    if (elements.Count == 0)
                    {
                        throw new PageHelper.RetryException();
                    }
                });

            return new ValueTask();
        }

        public void NavigateToBonusPoints()
        {
            _webDriver.FindElement(By.CssSelector(_bonusPointsLinkSelector))
                .Click();

            //Wait until lastTorrentsRows are unloaded
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_lastTorrentRowsSelector));
                    if (elements.Count != 0)
                    {
                        throw new PageHelper.RetryException();
                    }
                });
        }
    }
}
