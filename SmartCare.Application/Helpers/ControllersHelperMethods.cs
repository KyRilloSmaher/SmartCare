using Microsoft.AspNetCore.Mvc;
using SmartCare.Application.Handlers.ResponseHandler;
using System.Net;

namespace SmartCare.API.Helpers
{
    public static class ControllersHelperMethods
    {
        public static ObjectResult FinalResponse<T>(Response<T> response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new OkObjectResult(response);
                case HttpStatusCode.Created:
                    return new CreatedResult(string.Empty, response);
                case HttpStatusCode.Unauthorized:
                    return new UnauthorizedObjectResult(response);
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(response);
                case HttpStatusCode.NotFound:
                    return new NotFoundObjectResult(response);
                case HttpStatusCode.Accepted:
                    return new AcceptedResult(string.Empty, response);
                case HttpStatusCode.UnprocessableEntity:
                    return new UnprocessableEntityObjectResult(response);
                default:
                    return new BadRequestObjectResult(response);
            }
        }
        //public static string GetHtmlTemplate(string templateName, Dictionary<string, string>? placeholders = null)
        //{
        //    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"{templateName}.html");

        //    if (!File.Exists(templatePath))
        //    {
        //        throw new FileNotFoundException($"HTML template not found: {templatePath}");
        //    }

        //    var htmlContent = File.ReadAllText(templatePath);

        //    // Replace placeholders if provided
        //    if (placeholders != null)
        //    {
        //        foreach (var placeholder in placeholders)
        //        {
        //            htmlContent = htmlContent.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        //        }
        //    }

        //    return htmlContent;
        //}
    }
}

