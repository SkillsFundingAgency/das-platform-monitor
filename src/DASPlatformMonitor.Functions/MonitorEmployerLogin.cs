using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DASPlatformMonitor.Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace DASPlatformMonitor.Functions
{
    public class MonitorEmployerLogin
    {
        private readonly ILogger _log;
        private readonly IConfiguration _configuration;
        private readonly IWebDriverService _webDriverService;

        public MonitorEmployerLogin(ILogger log, IConfiguration configuration, IWebDriverService webDriverService)
        {
            _log = log;
            _configuration = configuration;
            _webDriverService = webDriverService;
        }

        [FunctionName("MonitorEmployerLogin")]
        public void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ExecutionContext context)
        {
            try
            {
                var employerUrl = _configuration.GetValue<string>("EmployerUrl") ?? throw new Exception($"Could not find employerUrl config");
                var employerUser = _configuration.GetValue<string>("EmployerUser") ?? throw new Exception($"Could not find employerUser config");
                var employerPassword = _configuration.GetValue<string>("EmployerPassword") ?? throw new Exception($"Could not find employerPassword config");

                _log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

                using (var driver = _webDriverService.InitializeChromeDriver())
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
    }
}