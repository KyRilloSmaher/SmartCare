using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDBContext _context;

        public PaymentRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByPaymentProviderReferenceIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProviderReferenceId == paymentIntentId);
        }

        public async Task<Payment?> GetPendingPaymentByOrderIdAsync(Guid orderId, bool trackChanges = false)
        {
            var query = _context.Payments.Where(p => p.OrderId == orderId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId, bool trackChanges = false)
        {
            var query = _context.Payments.Where(p => p.OrderId == orderId);

            return trackChanges
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }
        public IQueryable<Payment> GetPaymentsQueryable(bool trackChanges = false)
        {
            var query = _context.Payments.AsQueryable();
            return trackChanges ? query : query.AsNoTracking();
        }
    }
}