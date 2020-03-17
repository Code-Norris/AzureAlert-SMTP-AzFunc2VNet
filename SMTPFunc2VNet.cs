using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.WebHooks;
using Serilog.Core;
using Newtonsoft.Json.Linq;

namespace AzureAlert.SMTP
{
    public class SMTPFunc2VNet
    {
        public SMTPFunc2VNet(AppSettings appSettings, Logger logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        [FunctionName("SMTPFunc2VNet")]
        public async Task<IActionResult>Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
             ILogger funcTracer)
        {
            try
            {
                funcTracer.LogInformation(_appSettings.AAF_SMTPServerIP); //azfunc trace
                funcTracer.LogInformation(_appSettings.AAF_SMTPServerUserName); //azfunc trace
                funcTracer.LogInformation(_appSettings.AAF_SMTPServerPassword); //azfunc trace
                funcTracer.LogInformation(_appSettings.AAF_RecipientMailAddresses); //azfunc trace
                funcTracer.LogInformation(_appSettings.AAF_FromMailAddress); //azfunc trace
                funcTracer.LogInformation(_appSettings.AAF_MailSubject); //azfunc trace
                funcTracer.LogInformation(_appSettings.AAF_SMTPPort.ToString());

                _logger.Information(_appSettings.AAF_SMTPServerIP);
                 _logger.Information(_appSettings.AAF_SMTPServerUserName);
              
                _logger.Information("SMTPFunc2VNet-AMR triggered");

                //https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-common-schema-definitions
                //parse to common schema alert

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var azureAlertCAS = DeserializeAlertJson(requestBody);

                string mailBody = GenerateMailBody(azureAlertCAS);

                SendEmail(mailBody, funcTracer);

                string responseMessage = $"SMTP WebHook executed successfully. {requestBody}";
                
                return new  OkObjectResult(responseMessage);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return new StatusCodeResult(500);
            }
        }

        private AzureAlertCAS DeserializeAlertJson(string jsonAlert)
        {
           JObject cas = JObject.Parse(jsonAlert);
           JProperty data = cas.Property("data");
           var aaCas = JsonConvert.DeserializeObject<AzureAlertCAS>(data.Value.ToString());
           return aaCas;
        }

        private string GenerateMailBody(AzureAlertCAS alert)
        {
            var mailBody = new MailBody()
            {
               AlertRuleName = alert.Essentials.alertRule,
               Severity = alert.Essentials.severity,
               FiredDateTime = alert.Essentials.firedDateTime,
               ResolvedDateTime = alert.Essentials.resolvedDateTime,
               Description = alert.Essentials.description,
               MonitorCondition = alert.Essentials.monitorCondition,
               AlertResourceIds = alert.Essentials.alertTargetIDs,
               Conditions = alert.AlertContext.condition.allOf
            };

            string mailBodyText = mailBody.GenerateMailBody();

            return mailBodyText;
        }

        private void SendEmail(string emailBody, ILogger funcTracer)
        {
            try 
            {
                funcTracer.LogInformation($"SMTPFunc2VNet-AMR Sending email to: {_appSettings.AAF_RecipientMailAddresses}");
                _logger.Information
                    ($"SMTPFunc2VNet-AMR Sending email to: {_appSettings.AAF_RecipientMailAddresses}");

                SmtpClient client = new SmtpClient
                    (_appSettings.AAF_SMTPServerIP, _appSettings.AAF_SMTPPort);
                client.UseDefaultCredentials = false;
                client.Credentials =
                    new NetworkCredential(_appSettings.AAF_SMTPServerUserName, _appSettings.AAF_SMTPServerPassword);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_appSettings.AAF_FromMailAddress);

                SetMultipleMailRecipients(mailMessage);
                
                mailMessage.Body = emailBody;
                mailMessage.Subject = _appSettings.AAF_MailSubject;

                client.Send(mailMessage);
                
                 funcTracer.LogInformation($"Email sent to {_appSettings.AAF_RecipientMailAddresses}");
                 _logger.Information($"Email sent to: {_appSettings.AAF_RecipientMailAddresses}");
            }
            catch(Exception ex)
            {
                funcTracer.LogError(ex, ex.Message);
                _logger.Error(ex, ex.Message);
            }
        }

        private void SetMultipleMailRecipients(MailMessage email) 
        {
            if(!string.IsNullOrEmpty(_appSettings.AAF_RecipientMailAddresses))
            {
                var addrs = _appSettings.AAF_RecipientMailAddresses.Split(';');

                foreach(var addr in addrs)
                {
                    email.To.Add(addr);
                }
            }
            else
                throw new ArgumentException("No recipient address configured in env vars");

            
        }

        private AppSettings _appSettings;
        private Logger _logger;
    }
}
