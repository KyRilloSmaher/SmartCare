using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Helpers
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; }
        public string IntegrationId { get; set; }
        public string WebhookSecret { get; set; }
        public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
    }
}
