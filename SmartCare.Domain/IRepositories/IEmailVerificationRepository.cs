using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IEmailVerificationRepository
    {
        public Task AddVerificationAsync(string email, string code, TimeSpan validFor);
        public Task<EmailVerification?> GetByEmailAndCodeAsync(string email, string code);
        public Task<EmailVerification?> GetVerificationAsync(string email);
        public Task RemoveAsync(EmailVerification entity);
        public Task RemoveExpiredAsync();
    }
}
