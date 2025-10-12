using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace SmartCare.API.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                // Structured logging
                _logger.LogError(@"
***********************************************************************************************
Error Message ==> {Message}
Stack Trace   ==> {StackTrace}
***********************************************************************************************",
                    error.Message, error.StackTrace);

                var response = context.Response;
                response.ContentType = "application/json";

                // prepare a consistent ErrorsBag dictionary
                var errorsBag = new Dictionary<string, List<string>>();

                var responseModel = new Response<string>
                {
                    Succeeded = false,
                    ErrorsBag = errorsBag,
                    Message = error.Message
                };

                switch (error)
                {
                    case UnauthorizedAccessException:
                        responseModel.StatusCode = HttpStatusCode.Unauthorized;
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorsBag["Authentication"] = new List<string> { "Unauthorized access." };
                        break;

                    case ValidationException validationException:
                        responseModel.StatusCode = HttpStatusCode.UnprocessableEntity;
                        response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

                        // Group validation failures by property name
                        var grouped = validationException.Errors
                            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "General" : e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).Distinct().ToList()
                            );

                        responseModel.Message = "Validation failed.";
                        responseModel.ErrorsBag = grouped;
                        break;

                    case KeyNotFoundException keyNotFoundEx:
                        responseModel.StatusCode = HttpStatusCode.NotFound;
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        errorsBag["NotFound"] = new List<string> { keyNotFoundEx.Message };
                        break;

                    case DbUpdateException dbException:
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        response.StatusCode = (int)HttpStatusCode.BadRequest;

                        var dbMsg = dbException.InnerException?.Message ?? dbException.Message;
                        errorsBag["Database"] = new List<string> { dbMsg };
                        responseModel.Message = "A database error occurred.";
                        break;

                    default:
                        // Generic errors
                        responseModel.StatusCode = HttpStatusCode.InternalServerError;
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        var genericMsg = error.Message + (error.InnerException != null ? " | " + error.InnerException.Message : string.Empty);
                        errorsBag["Error"] = new List<string> { genericMsg };
                        responseModel.Message = "An unexpected error occurred.";
                        break;
                }

                // Ensure ErrorsBag is never null
                responseModel.ErrorsBag ??= new Dictionary<string, List<string>>();

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = JsonSerializer.Serialize(responseModel, options);

                await response.WriteAsync(result);
            }
        }
    }
}
