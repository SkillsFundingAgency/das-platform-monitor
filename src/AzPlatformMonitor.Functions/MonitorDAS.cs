using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AzPlatformMonitor.Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using NLog;
using OpenQA.Selenium;
using System.Linq;
using Newtonsoft.Json;

namespace AzPlatformMonitor.Functions
{
    public class MonitorDAS
    {
        private readonly ILogger _log;
        private readonly IConfiguration _configuration;
        private readonly IWebDriverService _webDriverService;

        public MonitorDAS(ILogger log, IConfiguration configuration, IWebDriverService webDriverService)
        {
            _log = log;
            _configuration = configuration;
            _webDriverService = webDriverService;
        }

        [FunctionName("MonitorDAS_TimerStart")]
        public async Task TimerStart(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer,
            [OrchestrationClient]DurableOrchestrationClient starter)
        {            
            var existingInstance = await starter.GetStatusAsync("1");
            if (existingInstance == null || existingInstance.RuntimeStatus != OrchestrationRuntimeStatus.Running)
            {
                var instanceId = await starter.StartNewAsync("MonitorDAS", "1",null);
                _log.Info($"Started orchestration with ID = '{instanceId}'.");
            } else
            {
                _log.Warn("MonitorDAS is already orchestrated");
            }
        }

        [FunctionName("MonitorDAS")]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            try
            {
                var employerUrl = _configuration.GetValue<string>("EmployerUrl");
                var employerUser = _configuration.GetValue<string>("EmployerUser");
                var employerPassword = _configuration.GetValue<string>("EmployerPassword");

                _log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

                List<Cookie> loginCookies = null;
                string loggedInUrl = null;
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
                    
                    if (!(driver.Url.StartsWith("https://accounts.manage-apprenticeships.service.gov.uk/accounts/") && driver.Url.EndsWith("/teams")))
                    {
                        _log.Error("Login test failed");
                        throw new Exception("Login test failed");
                    }

                    loginCookies = driver.Manage().Cookies.AllCookies.ToList();
                    loggedInUrl = driver.Url;
                }
                var outputs = new List<Task>();

                var myCookies = loginCookies.Select(c => new MyCookie() { Domain = c.Domain, Expiry = c.Expiry, Name = c.Name, Path = c.Path, Value = c.Value }).ToList();
                var activityDto = new ActivityDTO() { BaseUrl = employerUrl, LoggedInUrl = loggedInUrl, LoginCookies = myCookies };

                outputs.Add(context.CallActivityAsync<Task>("MonitorDAS_Commitments", activityDto));
                //outputs.Add(context.CallActivityAsync("MonitorDAS_SomethingElse", activityDto))
                await Task.WhenAll(outputs);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }            
        }

        [FunctionName("MonitorDAS_Commitments")]
        public async Task TestCommitments([ActivityTrigger] ActivityDTO driverDetails)
        {
            _log.Info(driverDetails.BaseUrl);
            _log.Info(driverDetails.LoginCookies.First().Name);
            using (var driver = _webDriverService.InitializeChromeDriver())
            {
                // Start screen
                _log.Info($"Start Url: {driverDetails.BaseUrl}");
                driver.Navigate().GoToUrl(driverDetails.BaseUrl);
                _log.Info($"Current location: {driver.Url}");

                var newCookieJar = driver.Manage().Cookies;
                newCookieJar.DeleteAllCookies();
                foreach(var cookie in driverDetails.LoginCookies)
                {                   
                    newCookieJar.AddCookie(new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Expiry));
                }

                // Start screen
                _log.Info($"Start Url: {driverDetails.LoggedInUrl}");
                driver.Navigate().GoToUrl(driverDetails.LoggedInUrl);
                _log.Info($"Current location: {driver.Url}");

                // Go to commitments
                var buttonElement = driver.FindElement(By.CssSelector("#global-nav-links > li:nth-child(4) > a"));
                buttonElement.Click();
                _log.Info($"Current location: {driver.Url}");
            }
        }

        public class ActivityDTO
        {
            public List<MyCookie> LoginCookies { get; set; }
            public string BaseUrl { get; set; }
            public string LoggedInUrl { get; set; }
        }

        public class MyCookie
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
            public DateTime? Expiry { get; set; }
        }
    }
}