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
         Task<Payment?> GetByPaymentProviderReferenceIdAsync(string paymentIntentId);

        Task<Payment?> GetPendingPaymentByOrderIdAsync(Guid orderId, bool trackChanges = false);

         IQueryable<Payment> GetPaymentsQueryable(bool trackChanges = false);

        Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId, bool trackChanges = false);
    }
}
