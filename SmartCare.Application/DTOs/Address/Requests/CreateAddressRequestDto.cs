using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Address.Requests
{
    public class CreateAddressRequestDto
    {
        public string address { get; set; }
        public string Label { get; set; }
        public string AdditionalInfo { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsPrimary { get; set; }
    }
}
