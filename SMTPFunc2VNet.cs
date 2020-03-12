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

                _logger.Information(_appSettings.AAF_SMTPServerIP);
                 _logger.Information(_appSettings.AAF_SMTPServerUserName);
              
                _logger.Information("SMTPFunc2VNet-AMR triggered");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                //dynamic data = JsonConvert.DeserializeObject(requestBody);

                _logger.Information($"SMTPFunc2VNet-AMR alert data received: {requestBody}");

                //var alert = JsonConvert.DeserializeObject<AzureAlertNotification>(requestBody);

                SendEmail(requestBody, funcTracer);

                string responseMessage = $"SMTP WebHook executed successfully. {requestBody}";
                
                return new  OkObjectResult(responseMessage);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return new StatusCodeResult(500);
            }
        }

        private void SendEmail(string emailBody, ILogger funcTracer)
        {
            try 
            {
                funcTracer.LogInformation($"SMTPFunc2VNet-AMR Sending email to: {_appSettings.AAF_RecipientMailAddresses}");
                _logger.Information
                    ($"SMTPFunc2VNet-AMR Sending email to: {_appSettings.AAF_RecipientMailAddresses}");

                SmtpClient client = new SmtpClient(_appSettings.AAF_SMTPServerIP);
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
