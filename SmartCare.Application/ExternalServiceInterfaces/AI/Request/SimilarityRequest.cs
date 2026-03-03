using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Request
{
    public record SimilarityRequest(
    string ProductId,
    int TopK,
    double? ScoreThreshold,
    bool ExcludeSelf);
}
