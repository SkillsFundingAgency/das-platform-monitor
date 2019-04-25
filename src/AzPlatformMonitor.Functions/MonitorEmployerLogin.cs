using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AzPlatformMonitor.Functions
{
    public class MonitorEmployerLogin
    {
        private static IWebDriver driver;
        private readonly ILogger _log;
        private readonly IConfiguration _configuration;

        public MonitorEmployerLogin(ILogger log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        [FunctionName("MonitorEmployerLogin")]
        public void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ExecutionContext context)
        {
            try
            {
                var employerUrl = _configuration.GetValue<string>("EmployerUrl");
                var employerUser = _configuration.GetValue<string>("EmployerUser");
                var employerPassword = _configuration.GetValue<string>("EmployerPassword");

                _log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

                using (driver = InitializeChromeDriver(context))
                {
                    // Start screen
                    _log.Info($"Start Url: {employerUrl}");
                    driver.Navigate().GoToUrl(employerUrl);
                    _log.Info($"Current location: {driver.Url}");

                    var buttonElement = driver.FindElement(By.Id("service-start"));
                    buttonElement.Click();
                    _log.Info($"Current location: {driver.Url}");

                    // Have you used this service before?
                    var radioButtonElement = driver.FindElement(By.CssSelector("label[for='used-service-before']"));
                    radioButtonElement.Click();

                    var continueButtonElement = driver.FindElement(By.Id("submit-button"));
                    continueButtonElement.Click();
                    _log.Info($"Current location: {driver.Url}");

                    // Login form
                    var usernameInputElement = driver.FindElement(By.Id("EmailAddress"));
                    usernameInputElement.SendKeys(employerUser);

                    var passwordInputElement = driver.FindElement(By.Id("Password"));
                    passwordInputElement.SendKeys(employerPassword);
                    _log.Info($"Current location: {driver.Url}");

                    var signInElement = driver.FindElement(By.Id("button-signin"));
                    signInElement.Click();
                    _log.Info($"Current location: {driver.Url}");

                    // Assert
                    var expectedTextOnSuccess = driver.FindElement(By.XPath("//*[contains(text(), 'Set up your account')]"));

                    if (expectedTextOnSuccess.Text != "Set up your account")
                    {
                        _log.Error("Login test failed");
                        throw new Exception("Login test failed");
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public static string GetEnvironmentVariable(String Name)
        {
            var environmentVariable = Environment.GetEnvironmentVariable(Name, EnvironmentVariableTarget.Process);
            if (String.IsNullOrEmpty(environmentVariable))
            {
                throw new Exception($"Could not find an environment variable [{Name}]");
            }
            return environmentVariable;
        }

        public static IWebDriver InitializeChromeDriver(ExecutionContext context)
        {
            var driverFileName = "chromedriver.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                driverFileName = "chromedriver";
            }

            var driverExecutableFilePath = $"{context.FunctionAppDirectory}/{driverFileName}";

            ChromeOptions options = new ChromeOptions();
            options.AddArguments("window-size=1200x600");
            options.AddArguments("no-sandbox");
            options.AddArguments("headless");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(context.FunctionAppDirectory, driverExecutableFilePath);
            driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(5));
            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}