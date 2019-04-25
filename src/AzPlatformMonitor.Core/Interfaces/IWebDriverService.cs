using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzPlatformMonitor.Core.Interfaces
{
    public interface IWebDriverService
    {
        IWebDriver InitializeChromeDriver();
    }
}
