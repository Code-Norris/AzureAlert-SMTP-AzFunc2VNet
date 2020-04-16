using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AzureAlert.SMTP
{
    public class MailBody
    {
        public MailBody()
        {
            InitCollections();
        }

        public string AlertRuleName { get; set; }

        public string Severity { get; set; }

        public string MonitorCondition { get; set; }

        public DateTime FiredDateTime { get; set; }

        public DateTime ResolvedDateTime { get; set; }

        public string Description { get; set; }

        public List<AllOf> Conditions { get; set; }

        public IEnumerable<string> AlertResourceIds { get; set; }

        public string GenerateMailBody()
        {
            string mailBody = GenerateMailBodyFormat();

            return mailBody;           
        }

        private IEnumerable<AffectedResource> GetAffectedResources()
        {
           var affectedRcs = new List<AffectedResource>();

            foreach(var arid in AlertResourceIds)
            {
                string[] props = arid.Split("/");
                int rscIdLength = props.Length;

                string resourceType = props[rscIdLength - 2];
                string resourceProvider = props[rscIdLength - 3];

                string resourceProviderAndType = resourceProvider + "/" + resourceType;
                string resourceGroup = props[4];
                string subscription = props[2];
                string resourceName = props[rscIdLength - 1];

                affectedRcs.Add(new AffectedResource(){
                    Subscription = subscription,
                    ResourceGroup = resourceGroup,
                    ResourceTypeAndProvider = resourceProviderAndType,
                    ResourceName = resourceName
                });
            }

            return affectedRcs;
        }

        private string GetAlertConditions()
        {
            var strBuilder = new StringBuilder();

            foreach(var con in Conditions)
            {
                strBuilder.Append(con.metricName + " " + con.@operator + " " + con.threshold);
                strBuilder.AppendLine();
            }

            return strBuilder.ToString();
        }
        
        private string GenerateMailBodyFormat()
        {
            var strBuilder = new StringBuilder();

             strBuilder.AppendLine();
             string supportMsg = severityMessages.FirstOrDefault(x => x.Key == Severity).Value;
             strBuilder.Append(supportMsg);
             strBuilder.AppendLine();
             strBuilder.AppendLine();
             
            strBuilder.Append("AlertRule: " + AlertRuleName);
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Severity: " + Severity);
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Description: " + Description);
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Alert fired at: " + FiredDateTime.AddHours(8).ToString("MM/dd/yyyy hh:mm tt"));
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            if(ResolvedDateTime == DateTime.MinValue)
                strBuilder.Append("Resolved at: Not resolved yet");
            else
                strBuilder.Append("Resolved at: " + ResolvedDateTime.AddHours(8).ToString("MM/dd/yyyy hh:mm tt"));
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Affected Resources:");
            strBuilder.AppendLine();

            var affectedRcs = GetAffectedResources();
            foreach(var rsc in affectedRcs)
            {
                strBuilder.Append
                    ("Subscription: " + subscriptionNames.GetValueOrDefault(rsc.Subscription));
                strBuilder.AppendLine();
                strBuilder.Append("ResourceGroup: " + rsc.ResourceGroup);
                strBuilder.AppendLine();
                strBuilder.Append("Resource Provider & Type: " + rsc.ResourceTypeAndProvider);
                strBuilder.AppendLine();
                strBuilder.Append("ResourceName: " + rsc.ResourceName);
                strBuilder.AppendLine();
                strBuilder.AppendLine();
            }

            string conditions = GetAlertConditions();
            strBuilder.Append("Alert Conditions:");
            strBuilder.AppendLine();
            strBuilder.AppendLine(conditions);
            strBuilder.AppendLine();

            return strBuilder.ToString();
        }

        private void InitCollections()
        {
            if(subscriptionNames.Count == 0)
            {
                subscriptionNames.Add("4d4ba1d0-92a2-49b7-8f98-85747a890a63", "mha0001-azr-012-spfprdpsg");
                subscriptionNames.Add("19c78837-5f06-4647-a264-62c846ddc607", "mha0001-azr-021-mhaprdcom");
            }

            if(severityMessages.Count == 0)
            {
                    severityMessages.Add("Sev4", String.Empty);
                    severityMessages.Add("Sev3", "For Severity 3, Severity 3 - NCS Level 1 Support - xxxx.xxx.xxx is currently looking into the issue.");
                    severityMessages.Add("Sev2", "For Severity 2, Severity 2 - NCS Level 2 Support - xxxx.xxx.xxx is currently looking into the issue.");
                    severityMessages.Add("Sev1", "For Severity 1, Severity 1 - NCS Level 2 Support - xxxx.xxx.xxx is currently looking into the issue.");
                    severityMessages.Add("Sev0", "For Severity 1, Severity 1 - NCS Level 2 Support - xxxx.xxx.xxx is currently looking into the issue.");
            }
        }

        private static Dictionary<string,string> subscriptionNames = new Dictionary<string,string>();
        private static Dictionary<string,string> severityMessages = new Dictionary<string,string>();
    }
}