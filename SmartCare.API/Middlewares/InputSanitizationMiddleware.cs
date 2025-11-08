using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SmartCare.API.Middlewares.SmartCare.API.Middlewares;

namespace SmartCare.API.Middlewares
{
    public class InputSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InputSanitizationMiddleware> _logger;
        private readonly Regex[] _patterns;
        private readonly InputSanitizationOptions _options;

        public InputSanitizationMiddleware(
            RequestDelegate next,
            ILogger<InputSanitizationMiddleware> logger,
            IOptions<InputSanitizationOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _patterns = _options.DangerousPatterns
                .Select(p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                .ToArray();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Sanitize Query Parameters
            foreach (var key in context.Request.Query.Keys.ToList())
            {
                var originalValue = context.Request.Query[key].ToString();
                var sanitized = Sanitize(originalValue);
                if (originalValue != sanitized)
                    _logger.LogWarning("Sanitized input in query param '{Key}'", key);
            }

            // Sanitize JSON body safely
            if (context.Request.ContentType?.Contains("application/json") == true &&
                context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var sanitizedJson = SanitizeJsonElement(doc.RootElement);
                    var sanitizedBody = JsonSerializer.Serialize(sanitizedJson);

                    if (sanitizedBody != body)
                        _logger.LogWarning("Sanitized malicious input from JSON body.");

                    var bytes = Encoding.UTF8.GetBytes(sanitizedBody);
                    context.Request.Body = new MemoryStream(bytes);
                    context.Request.Body.Position = 0;
                }
                catch (JsonException)
                {
                    // If JSON is invalid, skip sanitization and let the model binder handle the error
                    _logger.LogWarning("JSON body is invalid, skipping sanitization.");
                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);
        }

        private object SanitizeJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = SanitizeJsonElement(prop.Value);
                    }
                    return dict;

                case JsonValueKind.Array:
                    return element.EnumerateArray().Select(SanitizeJsonElement).ToList();

                case JsonValueKind.String:
                    return Sanitize(element.GetString());

                default:
                    return element.Clone(); // Keep numbers, booleans, null as-is
            }
        }

        private string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            input = input.Trim();
            if (input.Length > _options.MaxStringLength)
                input = input.Substring(0, _options.MaxStringLength);

            foreach (var regex in _patterns)
                input = regex.Replace(input, string.Empty);

            return input;
        }
    }
}
