using Microsoft.EntityFrameworkCore;
using Polly;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Repositories
{
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly ApplicationDBContext _context;
        public EmailVerificationRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task AddVerificationAsync(string email, string code, TimeSpan validFor)
        {
            var verification = new EmailVerification
            {
                Email = email,
                Token = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(validFor)
            };
            _context.EmailVerifications.Add(verification);

        }

        public async Task<EmailVerification?> GetByEmailAndCodeAsync(string email, string code)
        {
            return await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == email && v.Token == code && v.ExpiresAt > DateTime.UtcNow);
        }
        public async Task<EmailVerification?> GetVerificationAsync(string email)
        {
            return await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == email && v.ExpiresAt > DateTime.UtcNow);
        }

        public async Task RemoveAsync(EmailVerification entity)
        {
            _context.EmailVerifications.Remove(entity);

        }

        public async Task RemoveExpiredAsync()
        {
            var expired = await _context.EmailVerifications
                .Where(v => v.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.EmailVerifications.RemoveRange(expired);

        }
    }
}
