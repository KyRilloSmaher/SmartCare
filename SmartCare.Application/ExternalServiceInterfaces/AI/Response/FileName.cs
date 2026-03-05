using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Response
{
    public record ContradictionResult(
    string ProductId,
    int CandidatesChecked,
    double ContradictionThreshold,
    List<ContradictionResultItem> Contradictions,
    int Total);
}
