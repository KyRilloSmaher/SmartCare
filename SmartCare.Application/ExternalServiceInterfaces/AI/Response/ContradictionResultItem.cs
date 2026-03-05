using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Response
{
    public record ContradictionResultItem(
    string Id,
    double Score);
}
