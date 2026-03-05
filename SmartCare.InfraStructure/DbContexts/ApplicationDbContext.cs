using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using SmartCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging; // Add this

namespace SmartCare.InfraStructure.DbContexts
{
<<<<<<< HEAD
    public class ApplicationDBContext : IdentityDbContext<Client>
=======

    public class ApplicationDBContext : IdentityDbContext<ApplictionUser>
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
        private readonly ILogger<ApplicationDBContext> _logger; // Add logger

        public ApplicationDBContext()
        {
        }

        // Add ILogger parameter to constructor
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options,
                                   ILogger<ApplicationDBContext> logger = null) : base(options)
        {
            _logger = logger;
        }

        // Your existing DbSet properties...
        public DbSet<Address> Addresses { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OnlineOrder> OnlineOrders { get; set; }
        public DbSet<PickUpOrder> FromStoreOrders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Store> Stores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPT inheritance for Orders
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OnlineOrder>().ToTable("OnlineOrders");
<<<<<<< HEAD
            modelBuilder.Entity<FromStoreOrder>().ToTable("FromStoreOrders");

            // Configure AuditLog entity with method tracking
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);

                // Basic properties
                entity.Property(e => e.TableName)
                    .HasMaxLength(100)
                    ;
                entity.Property(e => e.Action)
                    .HasMaxLength(10);

                entity.Property(e => e.UserId)
                    .HasMaxLength(450);

                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("GETUTCDATE()");

                // JSON data
                entity.Property(e => e.OldValues)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.NewValues)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.ChangedColumns)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.StackTrace)
                    .HasColumnType("nvarchar(max)");

                // Method tracking properties
                entity.Property(e => e.MethodName)
                    .HasMaxLength(200);

                entity.Property(e => e.ClassName)
                    .HasMaxLength(200);

                entity.Property(e => e.Namespace)
                    .HasMaxLength(200);

                entity.Property(e => e.SourceFile)
                    .HasMaxLength(500);

                // IpAddress should be nullable or have default
                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45)
                    .HasDefaultValue("127.0.0.1");

                // Indexes for performance
                entity.HasIndex(e => new { e.TableName, e.Timestamp });
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.MethodName);
                entity.HasIndex(e => e.ClassName);
                entity.HasIndex(e => e.Namespace);
            });

=======
            modelBuilder.Entity<PickUpOrder>().ToTable("FromStoreOrders");
>>>>>>> 923f973e367ef4ffc1892f700b70f80352b1a3e8
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        #region Override SaveChanges with Auditing

        public override int SaveChanges()
        {
            var auditEntries = PrepareAuditEntries();
            var result = base.SaveChanges();
            SaveAuditLogs(auditEntries);
            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = PrepareAuditEntries();
            var result = await base.SaveChangesAsync(cancellationToken);
            await SaveAuditLogsAsync(auditEntries, cancellationToken);
            return result;
        }

        #endregion

        #region Audit Implementation with Logging

        private List<AuditEntry> PrepareAuditEntries()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            var now = DateTime.UtcNow;

            var stackTrace = new StackTrace(2, true);
            var frame = stackTrace.GetFrame(0);
            var method = frame?.GetMethod();

            var filteredStack = GetFilteredStackTrace();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                // Skip audit table itself
                if (entry.Entity is AuditLog)
                    continue;

                var auditEntry = new AuditEntry
                {
                    TableName = entry.Metadata.GetTableName(),
                    EntityName = entry.Entity.GetType().Name,
                    Action = GetAction(entry.State),
                    Timestamp = now,
                    EntityId = GetEntityId(entry.Entity),
                    UserId = GetCurrentUserId(),

                    MethodName = method?.Name,
                    ClassName = method?.DeclaringType?.Name,
                    Namespace = method?.DeclaringType?.Namespace,
                    SourceFile = frame?.GetFileName(),
                    LineNumber = frame?.GetFileLineNumber(),
                    StackTrace = filteredStack,

                    EntityReference = entry.Entity
                };

                CapturePropertyChanges(entry, auditEntry);

                // Skip useless UPDATEs
                if (auditEntry.Action == "UPDATE" &&
                    !auditEntry.ChangedColumns.Any())
                    continue;

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        private void CapturePropertyChanges(EntityEntry entry, AuditEntry auditEntry)
        {
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // Skip navigation properties, foreign keys, and temporary values
                if (property.Metadata.IsShadowProperty() ||
                    property.Metadata.IsForeignKey() ||
                    property.IsTemporary)
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue ?? DBNull.Value;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue ?? DBNull.Value;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue ?? DBNull.Value;
                            auditEntry.NewValues[propertyName] = property.CurrentValue ?? DBNull.Value;
                            auditEntry.ChangedColumns.Add(propertyName);
                        }
                        break;
                }
            }
        }

        private string GetFilteredStackTrace()
        {
            try
            {
                var stackTrace = new StackTrace(true);
                var frames = stackTrace.GetFrames();

                if (frames == null || frames.Length == 0)
                    return "NULL";

                var relevantFrames = frames
                    .Where(frame =>
                    {
                        var method = frame.GetMethod();
                        var declaringType = method?.DeclaringType;

                        if (declaringType == null)
                            return false;

                        var namespaceName = declaringType.Namespace ?? "";

                        // Filter out framework and internal methods
                        return !namespaceName.StartsWith("Microsoft.EntityFrameworkCore") &&
                               !namespaceName.StartsWith("System.") &&
                               !namespaceName.StartsWith("Microsoft.AspNetCore") &&
                               declaringType != typeof(ApplicationDBContext) &&
                               !method.Name.Contains("SaveChanges") &&
                               !method.Name.StartsWith("get_") &&
                               !method.Name.StartsWith("set_");
                    })
                    .Take(5)
                    .Select(frame =>
                    {
                        var method = frame.GetMethod();
                        var declaringType = method?.DeclaringType;
                        var methodName = method?.Name;
                        var className = declaringType?.Name;
                        var fileName = frame.GetFileName();
                        var lineNumber = frame.GetFileLineNumber();

                        var frameInfo = $"{className}.{methodName}";

                        if (!string.IsNullOrEmpty(fileName) && lineNumber > 0)
                            frameInfo += $" at {System.IO.Path.GetFileName(fileName)}:{lineNumber}";

                        return frameInfo;
                    })
                    .ToArray();

                return relevantFrames.Length > 0
                    ? string.Join(" ← ", relevantFrames)
                    : "NULL";
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error getting filtered stack trace");
                return "NULL";
            }
        }

        private void SaveAuditLogs(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
            {
                _logger?.LogDebug("No audit entries to save");
                return;
            }

            _logger?.LogInformation($"Saving {auditEntries.Count} audit entries");

            foreach (var auditEntry in auditEntries)
            {
                var auditLog = ConvertToAuditLog(auditEntry);
                AuditLogs.Add(auditLog);

                _logger?.LogDebug($"Added audit log for Order {auditEntry.EntityId}, Action: {auditEntry.Action}");
            }

            try
            {
                // Save audit logs in same transaction
                base.SaveChanges();
                _logger?.LogInformation($"Successfully saved {auditEntries.Count} audit entries");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to save audit entries. Count: {auditEntries.Count}");
                throw;
            }
        }

        private async Task SaveAuditLogsAsync(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries == null || auditEntries.Count == 0)
            {
                _logger?.LogDebug("No audit entries to save");
                return;
            }

            _logger?.LogInformation($"Saving {auditEntries.Count} audit entries asynchronously");

            foreach (var auditEntry in auditEntries)
            {
                var auditLog = ConvertToAuditLog(auditEntry);
                await AuditLogs.AddAsync(auditLog, cancellationToken);

                _logger?.LogDebug($"Added async audit log for Order {auditEntry.EntityId}, Action: {auditEntry.Action}");
            }

            try
            {
                await base.SaveChangesAsync(cancellationToken);
                _logger?.LogInformation($"Successfully saved {auditEntries.Count} audit entries asynchronously");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to save audit entries asynchronously. Count: {auditEntries.Count}");
                throw;
            }
        }

        private AuditLog ConvertToAuditLog(AuditEntry entry)
        {
            string Serialize(object obj) =>
                obj == null ? null : JsonSerializer.Serialize(obj);

            return new AuditLog
            {
                TableName = entry.TableName,
                EntityName = entry.EntityName,   // 🔥

                Action = entry.Action,
                EntityId = entry.EntityId,

                OldValues = Serialize(entry.OldValues),
                NewValues = Serialize(entry.NewValues),
                ChangedColumns = Serialize(entry.ChangedColumns),

                Timestamp = entry.Timestamp,
                UserId = entry.UserId ?? "system",

                MethodName = entry.MethodName,
                ClassName = entry.ClassName,
                Namespace = entry.Namespace,
                SourceFile = entry.SourceFile,
                LineNumber = entry.LineNumber ?? 0,
                StackTrace = entry.StackTrace,

                IpAddress = GetIpAddress()
            };
        }


        #endregion

        #region Helper Methods

        private bool IsOrderEntity(object entity)
        {
            return entity is Order ||
                   entity is OnlineOrder ||
                   entity is FromStoreOrder;
        }

        private string GetTableName(object entity)
        {
            return entity switch
            {
                OnlineOrder => "OnlineOrders",
                FromStoreOrder => "FromStoreOrders",
                _ => "Orders"
            };
        }

        private string GetAction(EntityState state)
        {
            return state switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };
        }

        private Guid? GetEntityId(object entity)
        {
            if (entity is Order order)
                return order.Id;

            // Use reflection for derived types
            var idProperty = entity.GetType().GetProperty("Id");
            return idProperty?.GetValue(entity) as Guid?;
        }

        private string GetCurrentUserId()
        {
            // TODO: Implement based on your authentication system
            // For now, return "system" for background jobs
            return "system";
        }

        private string GetIpAddress()
        {
            // TODO: Implement based on your HTTP context
            // For now, return a default value
            return "127.0.0.1";
        }

        #endregion

        #region Internal Helper Classes

        private class AuditEntry
        {
            public string TableName { get; set; }
            public string Action { get; set; }
            public string EntityName { get; set; }
            public DateTime Timestamp { get; set; }
            public string UserId { get; set; }
            public Guid? EntityId { get; set; }
            public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
            public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
            public List<string> ChangedColumns { get; } = new List<string>();
            public object EntityReference { get; set; }

            // Stack trace information
            public string MethodName { get; set; }
            public string ClassName { get; set; }
            public string Namespace { get; set; }
            public string SourceFile { get; set; }
            public int? LineNumber { get; set; }
            public string StackTrace { get; set; }
        }

        #endregion
    }
}