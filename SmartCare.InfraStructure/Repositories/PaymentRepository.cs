using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>,IPaymentRepository
    {
        private readonly ApplicationDBContext _context;
        public PaymentRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Payment?> GetBySessionIdAsync(string sessionId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.SessionId == sessionId);
        }
        public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
        }
        public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string paymentIntentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                payment.Status = status;
                payment.PaymentIntentId = paymentIntentId;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }

        }
    }
}
