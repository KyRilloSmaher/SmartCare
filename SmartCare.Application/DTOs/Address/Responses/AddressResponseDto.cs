using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Address.Responses
{
    public class AddressResponseDto
    {
        public Guid Id { get; set; }
        public string AddressLine { get; set; }
        public string Label { get; set; }
        public string AdditionalInfo { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsPrimary { get; set; }
    }
}
