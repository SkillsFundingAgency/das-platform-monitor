using AzPlatformMonitor.Core.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AzPlatformMonitor.Core.Services
{
    public class WebDriverService : IWebDriverService
    {
        private readonly string _driverFolder;

        public WebDriverService()
        {
            _driverFolder = Environment.CurrentDirectory;
        }

        public IWebDriver InitializeChromeDriver()
        {
            var driverFileName = "chromedriver.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                driverFileName = "chromedriver";
            }

            var driverExecutableFilePath = $"{_driverFolder}/{driverFileName}";

            ChromeOptions options = new ChromeOptions();
            options.AddArguments("window-size=1200x600");
            options.AddArguments("no-sandbox");
            options.AddArguments("headless");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(_driverFolder, driverExecutableFilePath);
            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(10));
            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}
