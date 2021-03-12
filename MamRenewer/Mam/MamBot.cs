using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Mam
{
    class MamBot
    {
        private readonly IWebDriver _webDriver;

        public MamBot(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public Task LoginAsync()
        {
            return null;
        }
    }
}
