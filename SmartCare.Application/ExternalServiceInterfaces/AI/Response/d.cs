using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Response
{
    public record SimilarProductsResult(
    string ProductId,
    int TopK,
    double? ScoreThreshold,
    List<SearchResultItem> Results,
    int Total);
}
