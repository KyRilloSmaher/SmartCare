using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Response
{
    public record SearchResultItem(
     string Id,
     double Score,
     Dictionary<string, object>? Metadata = null);
}
