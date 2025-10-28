using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Rates.Responses
{
    public class RateResponseDto
    {
        public Guid Id { get; set; }
        public ProductProjectionDTO product { get; set; }
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
