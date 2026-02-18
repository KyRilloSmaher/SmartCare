using MediatR;
using System;
using PaymentEntity = SmartCare.Domain.Entities.Payment;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Queries
{
    public record GetPaymentByOrderIdAsyncQuery(Guid orderId) : IRequest<PaymentEntity?>;
}
