using DASPlatformMonitor.Core.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DASPlatformMonitor.Core.Services
{
    public class WebDriverService : IWebDriverService
    {
        public IWebDriver InitializeChromeDriver(string functionAppDirectory)
        {
            var driverFileName = "chromedriver.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                driverFileName = "chromedriver";
            }

            var driverExecutableFilePath = $"{functionAppDirectory}/{driverFileName}";

            ChromeOptions options = new ChromeOptions();
            options.AddArguments("window-size=1200x600");
            options.AddArguments("no-sandbox");
            options.AddArguments("headless");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(functionAppDirectory, driverExecutableFilePath);
            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(30));
            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}
