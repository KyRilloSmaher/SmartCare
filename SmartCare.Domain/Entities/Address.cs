using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; set; } 
        public string ClientId { get; set; }
        public string address { get; set; }
        public string Label { get; set; }
        public string AdditionalInfo { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsPrimary { get; set; }
        public Client Client { get; set; }
        public ICollection<OnlineOrder> Orders { get; set; }
    }
}
