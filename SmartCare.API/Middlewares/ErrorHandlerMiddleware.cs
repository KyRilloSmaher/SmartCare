using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using SmartCare.Domain.Exceptions;

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
                    case System.UnauthorizedAccessException:
                        responseModel.StatusCode = HttpStatusCode.Unauthorized;
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorsBag["Authentication"] = new List<string> { "Unauthorized access." };
                        break;

                    case global::SmartCare.Domain.Exceptions.UnauthorizedException domainAuthException:
                        responseModel.StatusCode = HttpStatusCode.Unauthorized;
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorsBag["Authentication"] = new List<string> { domainAuthException.Message };
                        break;

                    case ForbiddenException forbiddenException:
                        responseModel.StatusCode = HttpStatusCode.Forbidden;
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        errorsBag["Forbidden"] = new List<string> { forbiddenException.Message };
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

                    case NotFoundException notFoundException:
                        responseModel.StatusCode = HttpStatusCode.NotFound;
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        errorsBag["NotFound"] = new List<string> { notFoundException.Message };
                        break;

                    case BadRequestException badRequestException:
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorsBag["BadRequest"] = new List<string> { badRequestException.Message };
                        break;

                    case DbUpdateException dbException:
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        response.StatusCode = (int)HttpStatusCode.BadRequest;

                        var dbMsg = dbException.InnerException?.Message ?? dbException.Message;
                        errorsBag["Database"] = new List<string> { dbMsg };
                        responseModel.Message = "A database error occurred.";
                        break;
                    case CachedException cacheException:
                        responseModel.StatusCode = HttpStatusCode.InternalServerError;
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorsBag["Cahce Error"] = new List<string> { cacheException.Message };
                        responseModel.Message = "Cache Service ERROR!";
                        break;
                    case DomainException domainException:
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorsBag["DomainError"] = new List<string> { domainException.Message };
                        responseModel.Message = "A domain rule was violated.";
                        break;
                    case AiCoreFeatureDisabledException featureDisabledEx:
                        responseModel.StatusCode = HttpStatusCode.ServiceUnavailable;
                        response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

                        errorsBag["AiCore"] = new List<string> { featureDisabledEx.Message };
                        responseModel.Message = "AI Core feature is currently unavailable.";
                        break;

                    case AiCoreValidationException aiValidationEx:
                        responseModel.StatusCode = HttpStatusCode.BadRequest;
                        response.StatusCode = (int)HttpStatusCode.BadRequest;

                        errorsBag["AiCoreValidation"] = new List<string> { aiValidationEx.Message };
                        responseModel.Message = "AI Core validation failed.";
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
