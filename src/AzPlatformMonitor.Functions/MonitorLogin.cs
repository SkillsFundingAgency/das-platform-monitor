using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AzPlatformMonitor.Functions
{
    public static class MonitorLogin
    {
        private static IWebDriver driver { get; set; }
        private static string _url = "https://manage-apprenticeships.service.gov.uk/";

        [FunctionName("MonitorLogin")]
        public static void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            //Configuration to Windows

            String driverExecutableFileName = $"{context.FunctionAppDirectory}\\chromedriver.exe";
            log.LogInformation($"Chromedriver should be at {driverExecutableFileName}");
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("window-size=1200x600");
            options.AddArguments("headless");
            options.AddArguments("no-sandbox");
                       
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(context.FunctionAppDirectory, driverExecutableFileName);
            driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(30));
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(40);
            driver.Manage().Window.Maximize();

            log.LogInformation($"Going to {_url}");
            driver.Navigate().GoToUrl(_url);
            var whereami = driver.Url;
            log.LogInformation($"I'm now at {whereami}");
            var buttonElement = driver.FindElement(By.Id("service-start"));
            log.LogInformation("Clicking start button");
            buttonElement.Click();
            var nowimat = driver.Url;
            log.LogInformation($"I got to {nowimat}");
        }
    }
}
