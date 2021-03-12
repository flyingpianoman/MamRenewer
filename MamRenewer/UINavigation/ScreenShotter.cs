using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.UINavigation
{
    public class ScreenShotter
    {
        private readonly IWebDriver _webDriver;
        private readonly string _screenshotDir;

        public ScreenShotter(IWebDriver webDriver, IConfiguration configuration)
        {
            _webDriver = webDriver;
            _screenshotDir = configuration.GetValue<string>("MamBot:ScreenshotDir");
        }

        public void TakeScreenshot(string name)
        {
            var ss = ((ITakesScreenshot)_webDriver).GetScreenshot();
            ss.SaveAsFile(Path.Combine(_screenshotDir, $"{name}.png"),
                ScreenshotImageFormat.Png);
        }

    }
}
