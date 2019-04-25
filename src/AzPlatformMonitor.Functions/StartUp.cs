using AzPlatformMonitor.Functions;
using AzPlatformMonitor.Infrastructure.Interfaces;
using AzPlatformMonitor.Infrastructure.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

[assembly: WebJobsStartup(typeof(Startup))]
namespace AzPlatformMonitor.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddLogging(logging =>
            {
                logging.AddFilter(level => true);
                logging.AddConsole();
            });

            builder.Services.AddSingleton<ITestService, TestService>();
        }
    }
}
