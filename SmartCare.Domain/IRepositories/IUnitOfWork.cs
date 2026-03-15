using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;

namespace SmartCare.Domain.IRepositories
{
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
        IPharmacistRepository Pharmacists { get; }
        IEmailVerificationRepository EmailVerifications { get; }
        ISalesRepository Sales { get; }

        // Identity Management
        UserManager<ApplictionUser> UserManager { get; }
        RoleManager<IdentityRole> RoleManager { get; }

        // Generic Repository access
        IGenericRepository<T> Repository<T>() where T : class;

        /// <summary>
        /// Saves all changes with automatic transaction management.
        /// All operations within the same SaveChanges call are atomic.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Explicitly begins a transaction for complex scenarios spanning multiple SaveChanges calls.
        /// Use only when you need to span operations across multiple SaveChanges calls.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits an explicit transaction.
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back an explicit transaction.
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        // Entity Tracking Management
        void DetachAllEntities();
        void DetachEntity<T>(T entity) where T : class;
        void AttachEntity<T>(T entity) where T : class;
        void SetEntityState<T>(T entity, EntityState state) where T : class;
        EntityState GetEntityState<T>(T entity) where T : class;
        Task ReloadEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

        // Cleanup
        Task CloseConnectionAsync();
    }
}