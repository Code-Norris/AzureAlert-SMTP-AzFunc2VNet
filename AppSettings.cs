namespace AzureAlert.SMTP
{
    public class AppSettings
    {
        public string AAF_FunctionAPIKey { get; set; } //in querystring passed from AzAlert Webhook
         
        public string AAF_AppInsightsKey { get; set; }

        public string AAF_SMTPServerIP { get; set; }

        public string AAF_SMTPServerUserName { get; set; }

        public string AAF_SMTPServerPassword { get; set; }

        public string AAF_MailSubject { get; set; } = "Azure Alert - Performance";

        public string AAF_FromMailAddress { get; set; }

        public string AAF_RecipientMailAddresses { get; set; } //semicolon delimited

        public int AAF_SMTPPort { get; set; }
    }
}