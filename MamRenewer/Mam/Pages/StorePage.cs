using MamRenewer.Polly;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Mam.Pages
{
    class StorePage
    {
        private const string _footerSelector = "div.footRow";
        private const string _vipStatusAccordionHeaderSelector = "#vipStatus";
        private const string _maxVipStatusExtensionButtonSelector = ".vipStatusContent button[value=\"max\"]";
        private const string _confirmDialogButtonsSelector = ".ui-dialog-buttonset button";
        private IWebDriver _webDriver;

        public StorePage()
        {
        }

        public ValueTask InitializeAsync(IWebDriver webDriver)
        {
            _webDriver = webDriver;

            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_footerSelector));
                    if (!elements.Any())
                    {
                        throw new PageHelper.RetryException();
                    }
                });

            return new ValueTask();
        }

        public void ToggleVipSection()
        {
            _webDriver.FindElement(By.CssSelector(_vipStatusAccordionHeaderSelector))
                .Click();

            //Wait for section to be expanded
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_maxVipStatusExtensionButtonSelector));
                    if (!elements.Any())
                    {
                        throw new PageHelper.RetryException();
                    }
                });
        }
        public void PurchaseMaxVipDuration()
        {
            _webDriver.FindElement(By.CssSelector(_maxVipStatusExtensionButtonSelector))
                .Click();

            //Wait for confirm dialog
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_confirmDialogButtonsSelector));
                    if (!elements.Any())
                    {
                        throw new PageHelper.RetryException();
                    }
                });

            _webDriver.FindElements(By.CssSelector(_confirmDialogButtonsSelector))
                .First() //The first button is the OK button
                .Click();

            //Wait until the extend button is gone
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_confirmDialogButtonsSelector));
                    if (elements.Any() && elements[0].Displayed)
                    {
                        throw new PageHelper.RetryException();
                    }
                });

        }
    }
}
