using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI
{
    public interface IAiServices
    {
        Task<SemanticSearchResult> SemanticSearchAsync(
       string query,
       int topK = 25,
       bool withVectors = false,
       CancellationToken ct = default);
        Task<VoiceSearchResponse> VoiceSearchAsync(
       IFormFile audioFile,
       int topK = 25,
       CancellationToken ct = default);

        Task<SimilarProductsResult> GetSimilarProductsAsync(
            Guid productId,
            int topK = 25,
            double? scoreThreshold = null,
            bool excludeSelf = true,
            CancellationToken ct = default);

        Task<ContradictionResult> CheckContradictionsAsync(
            Guid productId,
            List<Guid> candidateIds,
            double contradictionThreshold = -0.25,
            bool excludeSelf = true,
            CancellationToken ct = default);
        Task<AiAnswerResult> AskAIAsync(
            string Question ,
            string ingredient,
            IFormFile? audio = null,
            CancellationToken ct = default);
        Task<DrugExtractionResponse> DrugInformationExtractorAsync(
            IFormFile? image ,
            CancellationToken ct = default);
    }
}
