using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Analytics.Categories
{

        public class CategoryPerformanceDto
        {
            public Guid CategoryId { get; set; }
            public string Category { get; set; } = default!;
            public decimal Revenue { get; set; }
            public int Percentage { get; set; }
        }
    
}
