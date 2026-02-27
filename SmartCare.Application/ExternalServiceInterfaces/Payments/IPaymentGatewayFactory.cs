using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.Payments
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGetway Resolve(PaymentMethod provider);
    }
}
