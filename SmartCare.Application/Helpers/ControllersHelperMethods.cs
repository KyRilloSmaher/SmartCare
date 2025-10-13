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
        public static string HtmlTemplate(string title, string message, string? redirectUrl = null)
        {
            var redirectScript = redirectUrl != null
                ? $"<p>Redirecting to <a href='{redirectUrl}'>{redirectUrl}</a> in 5 seconds...</p>" +
                  $"<script>setTimeout(() => window.location.href = '{redirectUrl}', 5000);</script>"
                : "";

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #6dd5ed, #2193b0);
            color: #333;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
        }}
        .card {{
            background: white;
            border-radius: 16px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.1);
            padding: 40px;
            text-align: center;
            max-width: 400px;
            animation: fadeIn 0.8s ease-in-out;
        }}
        h1 {{
            color: #0078D7;
            margin-bottom: 15px;
        }}
        p {{
            font-size: 16px;
            margin-bottom: 10px;
        }}
        a {{
            color: #0078D7;
            text-decoration: none;
            font-weight: bold;
        }}
        @keyframes fadeIn {{
            from {{ opacity: 0; transform: translateY(20px); }}
            to {{ opacity: 1; transform: translateY(0); }}
        }}
    </style>
</head>
<body>
    <div class='card'>
        <h1>{title}</h1>
        <p>{message}</p>
        {redirectScript}
    </div>
</body>
</html>";
        }

    }
}
