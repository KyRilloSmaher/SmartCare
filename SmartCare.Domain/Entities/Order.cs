using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string? ClientId { get; set; }

        public OrderType OrderType { get; set; }
        public decimal TotalPrice { get; set; }

        private OrderStatus _status = OrderStatus.Pending;
        private List<StatusChangeRecord> _statusChanges = new();

        public OrderStatus Status
        {
            get => _status;
            set
            {
                // Skip if same value (EF Core calls setter during materialization)
                if (_status == value)
                    return;

                var oldStatus = _status;
                _status = value;

                // Record the change with full stack trace
                var change = new StatusChangeRecord
                {
                    OldStatus = oldStatus,
                    NewStatus = value,
                    ChangedAt = DateTime.UtcNow,
                    StackTrace = GetFilteredStackTrace()
                };

                _statusChanges.Add(change);

                // Log to console
                Console.WriteLine($"\n🚨 ORDER {Id} STATUS CHANGED: {oldStatus} -> {value}");
                Console.WriteLine($"Caller: {change.StackTrace}");
                Console.WriteLine($"Time: {change.ChangedAt:HH:mm:ss.fff}");

                // Also write to a file for persistence
                File.AppendAllText("order_status_changes.txt",
                    $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} | Order {Id} | {oldStatus} -> {value}\n" +
                    $"{change.StackTrace}\n---\n");
            }
        }

        private string GetFilteredStackTrace()
        {
            var stack = new System.Diagnostics.StackTrace(true);
            var frames = stack.GetFrames()
                .Where(f => f.GetFileName()?.Contains("SmartCare") == true)
                .Take(5)
                .Select(f => $"{f.GetMethod()?.DeclaringType?.Name}.{f.GetMethod()?.Name} " +
                            $"({System.IO.Path.GetFileName(f.GetFileName())}:{f.GetFileLineNumber()})");

            return string.Join(" <- ", frames);
        }

        private class StatusChangeRecord
        {
            public OrderStatus OldStatus { get; set; }
            public OrderStatus NewStatus { get; set; }
            public DateTime ChangedAt { get; set; }
            public string StackTrace { get; set; }
        }

        //Payment tracking
        public Guid PaymenId { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Payment? Payment { get; set; }
        public Client Client { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

}