using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;

namespace SmartCare.API.Services
{
    public class HtmlTemplateService
    {
        private readonly string _templatesPath;

        public HtmlTemplateService(IWebHostEnvironment env)
        {
            _templatesPath = Path.Combine(env.ContentRootPath, "Templates");
            Console.WriteLine($"ContentRoot: {env.ContentRootPath}");
            Console.WriteLine($"TemplatesPath: {_templatesPath}");

        }

        public string GetHtmlTemplate(
            string templateName,
            Dictionary<string, string>? placeholders = null)
        {
            var path = Path.Combine(_templatesPath, $"{templateName}.html");

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Template not found. Expected path: {path}");

            var html = File.ReadAllText(path);

            if (placeholders != null)
            {
                foreach (var item in placeholders)
                {
                    html = html.Replace(
                        $"{{{{{item.Key}}}}}",
                        item.Value ?? string.Empty
                    );
                }
            }

            return html;
        }
    }
}
