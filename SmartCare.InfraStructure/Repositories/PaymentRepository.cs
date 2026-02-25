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

        public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
        }

        public async Task<Payment?> GetByOrderIdAsync(Guid orderId, bool trackChanges = false)
        {
            var query = _context.Payments.Where(p => p.OrderId == orderId);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string paymentIntentId)
        {
            return Task.Run(async () =>
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment != null)
                {
                    payment.Status = status;
                    payment.PaymentIntentId = paymentIntentId;
                }
            });
        }

        public IQueryable<Payment> GetPaymentsQueryable(bool trackChanges = false)
        {
            var query = _context.Payments.AsQueryable();
            return trackChanges ? query : query.AsNoTracking();
        }
    }
}