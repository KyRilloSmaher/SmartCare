using MediatR;
using SmartCare.Application.DTOs.payment;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Commands
{
    public record PayOfflineAsyncCommand(string orderCode) : IRequest<Response<PaymentResult>>;
}
