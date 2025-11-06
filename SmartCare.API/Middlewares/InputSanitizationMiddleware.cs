using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            // Sanitize JSON body
            if (context.Request.ContentType?.Contains("application/json") == true &&
                context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();

                var sanitizedBody = Sanitize(body);
                if (body != sanitizedBody)
                    _logger.LogWarning("Sanitized malicious input from JSON body.");

                var bytes = System.Text.Encoding.UTF8.GetBytes(sanitizedBody);
                context.Request.Body = new MemoryStream(bytes);
            }

            await _next(context);
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
