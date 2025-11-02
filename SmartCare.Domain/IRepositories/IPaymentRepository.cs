using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment?> GetBySessionIdAsync(string sessionId);
        Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId);
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
    }
}
