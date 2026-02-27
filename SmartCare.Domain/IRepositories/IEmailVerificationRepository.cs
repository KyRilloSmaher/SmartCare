using Microsoft.EntityFrameworkCore;
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
        Task AddVerificationAsync(string email, string code, TimeSpan validFor);
        Task<EmailVerification?> GetValidVerificationAsync(string email, string code);
        Task<EmailVerification?> GetLatestByEmailAsync(string email);
        Task<bool> HasValidVerificationAsync(string email);
        Task RemoveExpiredAsync();
        IQueryable<EmailVerification> GetQueryable();
    }
}
