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
        public int IntegrationId { get; set; }
        public string WebhookSecret { get; set; }
        public string Publickey { get; set; }
        public string SecretKey { get; set; }
        public string HMAC { get; set; }
        public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
    }
}
