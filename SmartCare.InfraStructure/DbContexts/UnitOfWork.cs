using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.Repositories;

namespace SmartCare.Infrastructure.Data
{
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
        public IContradictionRepository Contradictions { get; }
        public IStoreRepository Stores { get; }
        public IRateRepository Rates { get; }
        public IProductRepository Products { get; }
        public IFavouriteRepository Favourites { get; }
        public IOrderRepository Orders { get; }
        public ICartRepository Carts { get; }
        public IReservationRepository Reservations { get; }
        public IInventoryRepository Inventories { get; }
        public IPaymentRepository Payments { get; }
        public IPharmacistRepository Pharmacists { get; }
        public IEmailVerificationRepository EmailVerifications { get; }
        public ISalesRepository Sales { get; }
        public IDeliveryRepository Deliveries { get; }

        // Identity Management
        public UserManager<ApplictionUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }

        #endregion

        #region Properties

        public bool HasActiveTransaction => _currentTransaction != null;
        public Guid? CurrentTransactionId => _currentTransaction?.TransactionId;
        public int TrackedEntitiesCount => _context.ChangeTracker.Entries().Count();

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
            IPaymentRepository paymentRepository,
            IPharmacistRepository pharmacistRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IContradictionRepository contradictionRepository,
            UserManager<ApplictionUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISalesRepository salesRepository,
            IDeliveryRepository deliveries)
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
            Pharmacists = pharmacistRepository ?? throw new ArgumentNullException(nameof(pharmacistRepository));
            EmailVerifications = emailVerificationRepository ?? throw new ArgumentNullException(nameof(emailVerificationRepository));
            Contradictions = contradictionRepository ?? throw new ArgumentNullException(nameof(contradictionRepository));

            Sales = salesRepository ?? throw new ArgumentNullException(nameof(salesRepository));
            // Initialize Identity managers
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            RoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            Deliveries = deliveries;
        }

        #endregion

        #region Repository Management

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

        #region Save Changes with Automatic Transaction

        /// <summary>
        /// Saves all changes with automatic transaction management.
        /// All operations within the same SaveChanges call are atomic.
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // If there's already a manual transaction, just save without managing transaction
            if (_currentTransaction != null)
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }

            // Use automatic transaction for atomic operations
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Detach entities to free memory
                DetachAllEntities();

                return result;
            }
            catch
            {
                // Transaction will be automatically rolled back when disposed
                DetachAllEntities();
                throw;
            }
        }

        #endregion

        #region Manual Transaction Management (for complex scenarios)

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

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
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

        #region Entity Tracking Management

        public void DetachAllEntities()
        {
            var entries = _context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }

        public void DetachEntity<T>(T entity) where T : class
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public void AttachEntity<T>(T entity) where T : class
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Attach(entity);
            }
        }

        public void SetEntityState<T>(T entity, EntityState state) where T : class
        {
            _context.Entry(entity).State = state;
        }

        public EntityState GetEntityState<T>(T entity) where T : class
        {
            return _context.Entry(entity).State;
        }

        public async Task ReloadEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            await _context.Entry(entity).ReloadAsync(cancellationToken);
        }

        #endregion

        #region Connection Management

        public async Task CloseConnectionAsync()
        {
            DetachAllEntities();
            await _context.Database.CloseConnectionAsync();
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Rollback();
                    _currentTransaction.Dispose();
                }

                DetachAllEntities();
                _context?.Dispose();
                _repositories.Clear();
            }
            _disposed = true;
        }

        #endregion
    }
}