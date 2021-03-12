using MamRenewer.Polly;
using MamRenewer.UINavigation;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MamRenewer.Mam.Pages
{
    class LoginPage
    {
        private const string _usernameSelector = "form[action=\"/takelogin.php\"] input[name=\"email\"]";
        private const string _pwdSelector = "form[action=\"/takelogin.php\"] input[name=\"password\"]";
        private const string _submitSelector = "form[action=\"/takelogin.php\"] input[type=\"submit\"]";
        private const string _footerSelector = "div.footRow";
        private IWebDriver _webDriver;

        public LoginPage()
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

        public void SetUserName(string username)
        {
            var usernameElement = _webDriver.FindElement(By.CssSelector(_usernameSelector));
            usernameElement
                .SendKeys(username);
        }

        public void SetPassword(string password)
        {
            _webDriver.FindElement(By.CssSelector(_pwdSelector))
                .SendKeys(password);
        }

        public void ClickLogin()
        {
            _webDriver.FindElement(By.CssSelector(_submitSelector))
                .Click();

            //Wait until pwd selector is unloaded
            PageHelper.WaitForWebElementPolicy
                .Execute(() =>
                {
                    var elements = _webDriver.FindElements(By.CssSelector(_pwdSelector));
                    if (elements.Any())
                    {
                        throw new PageHelper.RetryException();
                    }
                });
        }
    }
}
