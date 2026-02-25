using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.Repositories;
using System.Data;

namespace SmartCare.Infrastructure.Data
{


    /// <summary>
    /// Implementation of Unit of Work pattern for SmartCare application.
    /// Manages database context and coordinates repository operations with automatic transaction handling.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        #region Fields

        private readonly ApplicationDBContext _context;
        private IDbContextTransaction _currentTransaction;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed;
        private int _transactionCount;

        #endregion

        #region Repository Properties

        public IClientRepository Clients { get; }
        public IAddressRepository Addresses { get; }
        public ICategoryRepository Categories { get; }
        public ICompanyRepository Companies { get; }
        public IStoreRepository Stores { get; }
        public IRateRepository Rates { get; }
        public IProductRepository Products { get; }
        public IFavouriteRepository Favourites { get; }
        public IOrderRepository Orders { get; }
        public ICartRepository Carts { get; }
        public IReservationRepository Reservations { get; }
        public IInventoryRepository Inventories { get; }
        public IPaymentRepository Payments { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether there's an active transaction
        /// </summary>
        public bool HasActiveTransaction => _currentTransaction != null;

        /// <summary>
        /// Gets the current transaction ID if available
        /// </summary>
        public Guid? CurrentTransactionId => _currentTransaction?.TransactionId;

        /// <summary>
        /// Gets the number of tracked entities
        /// </summary>
        public int TrackedEntitiesCount => _context.ChangeTracker.Entries().Count();

        public IEmailVerificationRepository EmailVerifications => throw new NotImplementedException();

        #endregion

        #region Constructors

        public UnitOfWork(
            ApplicationDBContext context,
            IClientRepository clientRepository,
            IAddressRepository addressRepository,
            ICategoryRepository categoryRepository,
            ICompanyRepository companyRepository,
            IStoreRepository storeRepository,
            IRateRepository rateRepository,
            IProductRepository productRepository,
            IFavouriteRepository favouriteRepository,
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IReservationRepository reservationRepository,
            IInventoryRepository inventoryRepository,
            IPaymentRepository paymentRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();

            // Initialize repositories
            Clients = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            Addresses = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
            Categories = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            Companies = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            Stores = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            Rates = rateRepository ?? throw new ArgumentNullException(nameof(rateRepository));
            Products = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            Favourites = favouriteRepository ?? throw new ArgumentNullException(nameof(favouriteRepository));
            Orders = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            Carts = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            Reservations = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
            Inventories = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            Payments = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        #endregion

        #region Repository Management

        /// <summary>
        /// Gets a generic repository for the specified entity type
        /// </summary>
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>).MakeGenericType(type);
                var repository = Activator.CreateInstance(repositoryType, _context);
                _repositories[type] = repository;
            }

            return (IGenericRepository<T>)_repositories[type];
        }

        #endregion

        #region Auto-Transaction Save Changes

        /// <summary>
        /// Saves all changes with automatic transaction management
        /// Automatically wraps all changes in a transaction and rolls back on failure
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // If there's already a manual transaction, just save without managing transaction
            if (_currentTransaction != null)
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }

            // Otherwise, use automatic transaction
            return await SaveChangesWithTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Saves all changes within an automatic transaction
        /// Will rollback if any error occurs
        /// </summary>
        public async Task<int> SaveChangesWithTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Start a new transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Detach all entities after successful save to free memory
                DetachAllEntities();

                return result;
            }
            catch
            {
                // Transaction will be automatically rolled back when disposed
                // Clear change tracker to prevent partial state
                DetachAllEntities();
                throw;
            }
        }

        #endregion

        #region Manual Transaction Management

        /// <summary>
        /// Begins a new manual transaction
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                _transactionCount++;
                return;
            }

            _transactionCount = 1;
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Commits the current manual transaction
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            _transactionCount--;

            if (_transactionCount > 0)
            {
                return;
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);

                // Detach all entities after successful commit
                DetachAllEntities();
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Rolls back the current manual transaction
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);

                // Clear all tracked entities on rollback
                DetachAllEntities();
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
                _transactionCount = 0;
            }
        }

        #endregion

        #region Transaction Execution Helpers

        /// <summary>
        /// Executes an operation within a transaction
        /// Automatically handles commit/rollback
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            await ExecuteInTransactionAsync<object?>(async () =>
            {
                await operation();
                return null;
            }, cancellationToken);
        }

        /// <summary>
        /// Executes an operation within a transaction and returns a result
        /// Automatically handles commit/rollback
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            var shouldCloseTransaction = _currentTransaction == null;

            if (shouldCloseTransaction)
            {
                await BeginTransactionAsync(cancellationToken);
            }

            try
            {
                var result = await operation();

                if (shouldCloseTransaction)
                {
                    await CommitTransactionAsync(cancellationToken);
                }

                return result;
            }
            catch
            {
                if (shouldCloseTransaction && _currentTransaction != null)
                {
                    await RollbackTransactionAsync(cancellationToken);
                }
                throw;
            }
        }

        #endregion

        #region Entity Tracking Management

        /// <summary>
        /// Detaches all entities from the context to free memory
        /// Called automatically after successful SaveChanges
        /// </summary>
        public void DetachAllEntities()
        {
            var entries = _context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }

        /// <summary>
        /// Detaches a specific entity from the context
        /// </summary>
        public void DetachEntity<T>(T entity) where T : class
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        /// <summary>
        /// Attaches an entity to the context
        /// </summary>
        public void AttachEntity<T>(T entity) where T : class
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Attach(entity);
            }
        }

        /// <summary>
        /// Sets the state of an entity
        /// </summary>
        public void SetEntityState<T>(T entity, EntityState state) where T : class
        {
            _context.Entry(entity).State = state;
        }

        /// <summary>
        /// Gets the current state of an entity
        /// </summary>
        public EntityState GetEntityState<T>(T entity) where T : class
        {
            return _context.Entry(entity).State;
        }

        /// <summary>
        /// Reloads an entity from the database
        /// </summary>
        public async Task ReloadEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            await _context.Entry(entity).ReloadAsync(cancellationToken);
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Closes the database connection and releases all tracked entities
        /// </summary>
        public async Task CloseConnectionAsync()
        {
            DetachAllEntities();
            await _context.Database.CloseConnectionAsync();
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Disposes the unit of work and ensures all connections are closed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Rollback any active transaction
                if (_currentTransaction != null)
                {
                    _currentTransaction.Rollback();
                    _currentTransaction.Dispose();
                }

                // Detach all entities before closing connection
                DetachAllEntities();

                // Close and dispose context
                _context?.Dispose();
                _repositories.Clear();
            }
            _disposed = true;
        }

        void IUnitOfWork.ReloadEntityAsync<T>(T entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}