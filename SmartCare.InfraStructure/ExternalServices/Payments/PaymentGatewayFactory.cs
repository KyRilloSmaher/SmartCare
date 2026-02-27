using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Application.IServices;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.ExternalServices.Payments
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IEnumerable<IPaymentGetway> _gateways;

        public PaymentGatewayFactory(IEnumerable<IPaymentGetway> gateways)
        {
            _gateways = gateways;
        }

        public IPaymentGetway Resolve(PaymentMethod provider)
        {
            var gateway = _gateways.FirstOrDefault(g => g.Provider == provider);

            if (gateway is null)
                throw new NotSupportedException($"Payment provider '{provider}' is not supported.");

            return gateway;
        }
    }
}
