using System;
using AzPlatformMonitor.Infrastructure.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzPlatformMonitor.Functions
{
    public class MonitorAgentUtilisation
    {
        private readonly ITestService _testService;
        public MonitorAgentUtilisation(ITestService testService)
        {
            this._testService = testService ?? throw new ArgumentNullException(nameof(testService));
        }

        [FunctionName("MonitorAgentUtilisation")]
        public void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _testService.IDoNothing();
        }
    }
}
