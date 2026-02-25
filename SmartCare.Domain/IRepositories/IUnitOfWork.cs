
using Microsoft.EntityFrameworkCore;

namespace SmartCare.Domain.IRepositories
{
    /// <summary>
    /// Unit of Work pattern implementation for coordinating database operations
    /// and managing transactions across multiple repositories.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repository Properties
        IClientRepository Clients { get; }
        IAddressRepository Addresses { get; }
        ICategoryRepository Categories { get; }
        ICompanyRepository Companies { get; }
        IStoreRepository Stores { get; }
        IRateRepository Rates { get; }
        IProductRepository Products { get; }
        IFavouriteRepository Favourites { get; }
        IOrderRepository Orders { get; }
        ICartRepository Carts { get; }
        IReservationRepository Reservations { get; }
        IInventoryRepository Inventories { get; }
        IPaymentRepository Payments { get; }
        IEmailVerificationRepository EmailVerifications { get; }

        // Generic Repository access
        IGenericRepository<T> Repository<T>() where T : class;

        // Save Changes - Automatically handles transactions
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Manual Transaction Control (for complex scenarios)
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        // Bulk Operations with Auto-Transaction
        Task<int> SaveChangesWithTransactionAsync(CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

        // Entity Tracking Management
        void DetachAllEntities();
        void DetachEntity<T>(T entity) where T : class;
        void AttachEntity<T>(T entity) where T : class;
        void SetEntityState<T>(T entity, EntityState state) where T : class;
        EntityState GetEntityState<T>(T entity) where T : class;
        void ReloadEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

        // Cleanup
        Task CloseConnectionAsync();
    }
}
