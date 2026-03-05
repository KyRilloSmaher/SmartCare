using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Request
{
    public  record SemanticSearchRequest(
    string Query,
    int TopK,
    bool WithVectors);
}
