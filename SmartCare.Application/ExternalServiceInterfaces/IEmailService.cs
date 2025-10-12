using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
        Task<bool> SendConfirmationEmailAsync(string email, string callbackUrl);
        Task<bool> SendPasswordResetEmailAsync(string email, string subject, string message);

    }
}
