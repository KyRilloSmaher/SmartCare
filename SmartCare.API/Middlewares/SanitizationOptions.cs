namespace SmartCare.API.Middlewares
{
    namespace SmartCare.API.Middlewares
    {
        public class InputSanitizationOptions
        {
            public int MaxStringLength { get; set; } = 5000;
            public string[] DangerousPatterns { get; set; } = new[]
            {
            "<script.*?>.*?</script>",      // XSS scripts
            "(['\";]|--)",                  // SQL injection
            "(drop|delete|truncate|insert|update)\\s", // SQL commands
            "on\\w+\\s*=",                  // JS event handlers
            "javascript:"                   // JS URIs
        };
        }
    }

}
