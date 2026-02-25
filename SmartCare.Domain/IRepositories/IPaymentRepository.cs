using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
         Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId);

        Task<Payment?> GetByOrderIdAsync(Guid orderId, bool trackChanges = false);

         Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string paymentIntentId);
         IQueryable<Payment> GetPaymentsQueryable(bool trackChanges = false);
    
}
}
