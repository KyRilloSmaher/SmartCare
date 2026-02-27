using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;

namespace SmartCare.InfraStructure.Repositories
{
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly DbSet<EmailVerification> _dbSet;

        public EmailVerificationRepository(ApplicationDBContext context)
        {
            _context = context;
            _dbSet = _context.Set<EmailVerification>();
        }

        public async Task AddVerificationAsync(string email, string code, TimeSpan validFor)
        {
            var verification = new EmailVerification
            {
                Email = email,
                Token = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(validFor),
                
            };

            await _dbSet.AddAsync(verification);
        }

        public async Task<EmailVerification?> GetValidVerificationAsync(string email, string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.Email == email &&v.Token == code &&!v.IsUsed && v.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<EmailVerification?> GetLatestByEmailAsync(string email)
        {
            return await _dbSet
                .Where(v => v.Email == email)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasValidVerificationAsync(string email)
        {
            return await _dbSet
                .AnyAsync(v => v.Email == email &&
                              !v.IsUsed &&
                              v.ExpiresAt > DateTime.UtcNow);
        }



        public async Task RemoveExpiredAsync()
        {
            var expired = await _dbSet
                .Where(v => v.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            if (expired.Any())
                _dbSet.RemoveRange(expired);
        }

        public IQueryable<EmailVerification> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}