using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class ClientAnalyticsDto
    {
        public int TotalClients { get; set; }
        public int NewClients { get; set; }
        public int ReturningClients { get; set; }
    }

}
