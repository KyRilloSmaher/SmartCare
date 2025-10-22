using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Rates.Requests
{
    public class CreateRateRequestDto
    {
        public Guid ProductId { get; set; }
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
