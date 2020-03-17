using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AzureAlert.SMTP
{
    public class MailBody
    {
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

            strBuilder.Append("AlertRule: " + AlertRuleName);
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Severity: " + Severity);
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Description: " + Description);
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Alert fired at: " + FiredDateTime.ToString("MM/dd/yyyy hh:mm tt"));
            strBuilder.AppendLine();
            strBuilder.AppendLine();

             strBuilder.Append("Resolved at: " + ResolvedDateTime.ToString("MM/dd/yyyy hh:mm tt"));
            strBuilder.AppendLine();
            strBuilder.AppendLine();

            strBuilder.Append("Affected Resources:");
            strBuilder.AppendLine();

            var affectedRcs = GetAffectedResources();
            foreach(var rsc in affectedRcs)
            {
                strBuilder.Append("Subscription: " + rsc.Subscription);
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
            strBuilder.AppendLine();

            return strBuilder.ToString();
        }
    }
}