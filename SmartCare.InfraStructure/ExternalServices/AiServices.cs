
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.ExternalServiceInterfaces.AI.Request;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Domain.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartCare.InfraStructure.ExternalServices;

public class AiCoreService : IAiServices
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiCoreService> _logger;

    // snake_case ↔ PascalCase automatically on every call
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public AiCoreService(HttpClient httpClient, ILogger<AiCoreService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // ── Semantic Search ───────────────────────────────────────────────────────

    public async Task<SemanticSearchResult> SemanticSearchAsync(
        string query,
        int topK = 25,
        bool withVectors = false,
        CancellationToken ct = default)
    {
        _logger.LogInformation("SemanticSearch | query={Query} top_k={TopK}", query, topK);

        var payload = new SemanticSearchRequest(query, topK, withVectors);

        return await PostAsync<SemanticSearchRequest, SemanticSearchResult>(
            "api/v1/semantic-search", payload, ct);
    }
    // ── Voice Search ───────────────────────────────────────────────────────
    public async Task<VoiceSearchResponse> VoiceSearchAsync(
        IFormFile file,
        int topK = 25,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "VoiceSearch | File Size={Size} top_k={TopK}",
            file.Length, topK);

        var fields = new Dictionary<string, string>
            {
                { "topK", topK.ToString() }
            };

        var files = new Dictionary<string, IFormFile>
        {
            { "file", file }
        };

        return await PostMultipartAsync<VoiceSearchResponse>(
            "api/v1/voice-search",
            fields,
            files,
            ct);
    }

    // ── Similarity ────────────────────────────────────────────────────────────

    public async Task<SimilarProductsResult> GetSimilarProductsAsync(
        Guid productId,
        int topK = 10,
        double? scoreThreshold = null,
        bool excludeSelf = true,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "SimilarProducts | productId={ProductId} top_k={TopK} threshold={Threshold}",
            productId, topK, scoreThreshold);

        var payload = new SimilarityRequest(productId.ToString(), topK, scoreThreshold, excludeSelf);

        return await PostAsync<SimilarityRequest, SimilarProductsResult>(
            "api/v1/similarity/find", payload, ct);
    }

    // ── Contradictions ────────────────────────────────────────────────────────

    public async Task<ContradictionResult> CheckContradictionsAsync(
        Guid productId,
        List<Guid> candidateIds,
        double contradictionThreshold = -0.25,
        bool excludeSelf = true,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Contradictions | productId={ProductId} candidates={Count} threshold={Threshold}",
            productId, candidateIds.Count, contradictionThreshold);

        var payload = new ContradictionRequest(
            productId, candidateIds, contradictionThreshold, excludeSelf);

        return await PostAsync<ContradictionRequest, ContradictionResult>(
            "api/v1/contradictions/check", payload, ct);
    }
    // ── Chat ────────────────────────────────────────────────────────

    public async Task<AiAnswerResult> AskAIAsync(string Question, string ingredient, IFormFile? audio = null, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Chat | Question ={Question} ingredient={ingredient} File ={audio}",
            Question, ingredient, audio.Length);
        var fields = new Dictionary<string, string>
            {
                {
                 "ingredient", ingredient.ToString()
                },
                {
                  "question", Question.ToString()
                }
            };

        var files = new Dictionary<string, IFormFile>
        {
            { "file", audio }
        };

        return await PostMultipartAsync<AiAnswerResult>(
            "api/v1/chat",
            fields,
            files,
            ct);
    }

    // ── Shared POST helper ────────────────────────────────────────────────────

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        CancellationToken ct)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(endpoint, payload, JsonOptions, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for {Endpoint}", endpoint);
            throw new AiCoreException($"Network error calling {endpoint}", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Request timed out for {Endpoint}", endpoint);
            throw new AiCoreException($"Request timed out calling {endpoint}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "AI Core returned {StatusCode} for {Endpoint}: {Body}",
                response.StatusCode, endpoint, body);

            throw response.StatusCode switch
            {
                HttpStatusCode.ServiceUnavailable => new AiCoreFeatureDisabledException(endpoint, body),
                HttpStatusCode.BadRequest => new AiCoreValidationException(endpoint, body),
                _ => new AiCoreException(
                                                         $"AI Core error {(int)response.StatusCode} at {endpoint}: {body}")
            };
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);

        if (result is null)
            throw new AiCoreException($"Empty or null response body from {endpoint}");

        return result;
    }
    private async Task<TResponse> PostMultipartAsync<TResponse>(
    string endpoint,
    Dictionary<string, string>? formFields,
    Dictionary<string, IFormFile>? files,
    CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();

        // 🧾 Add normal fields
        if (formFields is not null)
        {
            foreach (var field in formFields)
            {
                content.Add(new StringContent(field.Value), field.Key);
            }
        }

        // 📁 Add files
        if (files is not null)
        {
            foreach (var fileEntry in files)
            {
                var file = fileEntry.Value;

                var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, fileEntry.Key, file.FileName);
            }
        }

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsync(endpoint, content, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while calling {Endpoint}", endpoint);
            throw new AiCoreException($"Network error calling {endpoint}", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout while calling {Endpoint}", endpoint);
            throw new AiCoreException($"Timeout calling {endpoint}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            _logger.LogError(
                "Multipart POST failed {StatusCode} for {Endpoint}: {Body}",
                response.StatusCode, endpoint, body);

            throw new AiCoreException(
                $"AI Core error {(int)response.StatusCode} at {endpoint}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);

        if (result is null)
            throw new AiCoreException($"Empty response body from {endpoint}");

        return result;
    }


}
