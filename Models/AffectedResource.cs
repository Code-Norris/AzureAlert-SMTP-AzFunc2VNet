using System.Text;

namespace AzureAlert.SMTP
{
    public class AffectedResource
    {
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string ResourceTypeAndProvider { get; set; }
        public string ResourceName { get; set; }
    }
}