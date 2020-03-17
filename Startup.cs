using System;
using System.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

[assembly: FunctionsStartup(typeof(AzureAlert.SMTP.Startup))]
namespace AzureAlert.SMTP
{

  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
        {
          //appsettings either from local file for dev, env var for 
           var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot config = configBuilder.Build();

            var appSettings = new AppSettings() {
                AAF_FunctionAPIKey = config.GetValue<string>("AAF_FunctionAPIKey"),
                AAF_AppInsightsKey = config.GetValue<string>("AAF_AppInsightsKey"),
                AAF_SMTPServerIP = config.GetValue<string>("AAF_SMTPServerIP"),
                AAF_SMTPServerUserName = config.GetValue<string>("AAF_SMTPServerUserName"),
                AAF_SMTPServerPassword = config.GetValue<string>("AAF_SMTPServerPassword"),
                AAF_MailSubject = config.GetValue<string>("AAF_MailSubject"),
                AAF_FromMailAddress = config.GetValue<string>("AAF_FromMailAddress"),
                AAF_RecipientMailAddresses = config.GetValue<string>("AAF_RecipientMailAddress"),
                AAF_SMTPPort = config.GetValue<int>("AAF_SMTPPort")
            };

            var logger = InitAppInsights(appSettings);

            builder.Services.AddSingleton<AppSettings>(prov => {
                return appSettings;
            });

            builder.Services.AddSingleton<Logger>(prov => {
                return logger;
            });

        }

        private Logger InitAppInsights(AppSettings settings){
          
          var appInsightsConfig = new TelemetryConfiguration();
          
          var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo
            .ApplicationInsights(settings.AAF_AppInsightsKey, new TraceTelemetryConverter())
            .CreateLogger();

          return logger;
        }
  }
}