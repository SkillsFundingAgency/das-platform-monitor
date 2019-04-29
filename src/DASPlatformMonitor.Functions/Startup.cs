using DASPlatformMonitor.Core.Interfaces;
using DASPlatformMonitor.Core.Services;
using DASPlatformMonitor.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Slack;
using NLog.Targets;
using System;

[assembly: WebJobsStartup(typeof(Startup))]
namespace DASPlatformMonitor.Functions
{
    public class Startup : IWebJobsStartup
    {
        private readonly IConfiguration _configuration;

        public Startup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.Configure<dynamic>(_configuration);

            var logger = ConfigureLogger();
            builder.Services.AddSingleton<ILogger>(logger);
            builder.Services.AddScoped<IWebDriverService, WebDriverService>();
        }

        public Logger ConfigureLogger()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget()
            {
                Name = "console",
                Layout = "${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}"
            };
            config.AddTarget(consoleTarget);
            config.AddRuleForAllLevels(consoleTarget);

            var fileTarget = new FileTarget()
            {
                Name = "localerrorfile",
                FileName = "${currentdir}/errors.log",
                Layout = "${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForOneLevel(LogLevel.Error, fileTarget);

            if (!string.IsNullOrWhiteSpace(_configuration.GetValue<string>("SlackWebhookUrl")))
            {
                var slackTarget = new SlackTarget()
                {
                    Name = "slack",
                    WebHookUrl = _configuration.GetValue<string>("SlackWebhookUrl"),
                    Layout = "${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}"
                    // Add Fields and such
                };
                config.AddTarget(slackTarget);
                config.AddRuleForOneLevel(LogLevel.Error, slackTarget);
            }

            InternalLogger.LogFile = "internal-nlog.txt";
            InternalLogger.LogLevel = LogLevel.Info;
            LogManager.ThrowExceptions = true;
            LogManager.Configuration = config;

            return LogManager.GetCurrentClassLogger();
        }
    }
}
