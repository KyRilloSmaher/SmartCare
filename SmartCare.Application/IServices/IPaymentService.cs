using SmartCare.Application.DTOs.Payment;
using SmartCare.Application.Handlers.ResponseHandler;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IPaymentService
    {
        Task<Response<Session>> HandleAsync(CreateCheckoutSessionRequest req);
        Task<Response<string>> MarkPaymentSuccessAsync(string orderId);
        Task<Response<string>> MarkPaymentFailureAsync(string orderId);

    }
}
