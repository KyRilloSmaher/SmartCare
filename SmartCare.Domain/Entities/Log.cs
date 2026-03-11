using System;
using System.Text.Json;

namespace SmartCare.Domain.Entities
{
    public class AuditLog
    {

            public int Id { get; set; }

            public string? TableName { get; set; }
            public string? EntityName { get; set; }   // 🔥 NEW

            public string? Action { get; set; }
            public Guid? EntityId { get; set; }

            public string? OldValues { get; set; }
            public string? NewValues { get; set; }
            public string? ChangedColumns { get; set; }

            public string? UserId { get; set; }
            public DateTime? Timestamp { get; set; }

            public string? MethodName { get; set; }
            public string? ClassName { get; set; }
            public string? Namespace { get; set; }
            public string? SourceFile { get; set; }
            public int? LineNumber { get; set; }
            public string? StackTrace { get; set; }

            public string? IpAddress { get; set; }
        


        // Helper properties (not mapped to database)
        public string GetFullMethodName()
        {
            if (!string.IsNullOrEmpty(Namespace) && !string.IsNullOrEmpty(ClassName))
                return $"{Namespace}.{ClassName}.{MethodName}";

            if (!string.IsNullOrEmpty(ClassName))
                return $"{ClassName}.{MethodName}";

            return MethodName ?? "Unknown";
        }

        public string GetSourceLocation()
        {
            if (!string.IsNullOrEmpty(SourceFile) && LineNumber.HasValue)
            {
                var fileName = System.IO.Path.GetFileName(SourceFile);
                return $"{fileName}:{LineNumber}";
            }

            return null;
        }

        public string GetCallingInfo()
        {
            var methodInfo = GetFullMethodName();
            var location = GetSourceLocation();

            return !string.IsNullOrEmpty(location)
                ? $"{methodInfo} ({location})"
                : methodInfo;
        }

        public T GetOldValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(OldValues))
                return default;

            try
            {
                var oldValuesDict = JsonSerializer.Deserialize<Dictionary<string, object>>(OldValues);
                if (oldValuesDict != null && oldValuesDict.TryGetValue(propertyName, out var value))
                {
                    return JsonSerializer.Deserialize<T>(value.ToString());
                }
            }
            catch
            {
                // Log error or handle gracefully
            }

            return default;
        }

        public T GetNewValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(NewValues))
                return default;

            try
            {
                var newValuesDict = JsonSerializer.Deserialize<Dictionary<string, object>>(NewValues);
                if (newValuesDict != null && newValuesDict.TryGetValue(propertyName, out var value))
                {
                    return JsonSerializer.Deserialize<T>(value.ToString());
                }
            }
            catch
            {
                // Log error or handle gracefully
            }

            return default;
        }

        public string[] GetChangedColumnsArray()
        {
            if (string.IsNullOrEmpty(ChangedColumns))
                return Array.Empty<string>();

            try
            {
                return JsonSerializer.Deserialize<string[]>(ChangedColumns) ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public string GetActionSummary()
        {
            return Action switch
            {
                "INSERT" => $"Created by {UserId} via {GetFullMethodName()}",
                "UPDATE" => $"Modified by {UserId} via {GetFullMethodName()}",
                "DELETE" => $"Deleted by {UserId} via {GetFullMethodName()}",
                _ => $"{Action} by {UserId} via {GetFullMethodName()}"
            };
        }
    }
}