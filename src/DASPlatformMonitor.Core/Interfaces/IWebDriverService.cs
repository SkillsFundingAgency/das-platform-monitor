using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace DASPlatformMonitor.Core.Interfaces
{
    public interface IWebDriverService
    {
        IWebDriver InitializeChromeDriver();
    }
}
