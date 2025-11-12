using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class FilterProductsDTo
    {
        public bool? OrderByName { get; set; } = null;
        public bool? OrderByPrice { get; set; } = null;
        public bool? OrderByRate { get; set; } = null;
        public float? FromRate { get; set; } = 0.0f;
        public float? ToRate { get; set; } = null;
        public decimal? FromPrice { get; set; } = 0.0m;
        public decimal? ToPrice { get; set; } = null;
    }
}
