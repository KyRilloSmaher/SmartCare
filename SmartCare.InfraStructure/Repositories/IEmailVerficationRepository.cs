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
                IsUsed = false
            };

            await _dbSet.AddAsync(verification);
        }

        public async Task<EmailVerification?> GetValidVerificationAsync(string email, string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.Email == email &&
                                         v.Token == code &&
                                         !v.IsUsed &&
                                         v.ExpiresAt > DateTime.UtcNow);
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

        public Task MarkAsUsedAsync(int verificationId)
        {
            return Task.Run(async () =>
            {
                var verification = await _dbSet.FindAsync(verificationId);
                if (verification != null)
                {
                    verification.IsUsed = true;
                    verification.UsedAt = DateTime.UtcNow;
                    _dbSet.Update(verification);
                }
            });
        }

        public async Task<bool> MarkAsUsedAsync(string email, string token)
        {
            var verification = await _dbSet
                .Where(v => v.Email == email && v.Token == token && !v.IsUsed)
                .FirstOrDefaultAsync();

            if (verification != null)
            {
                verification.IsUsed = true;
                verification.UsedAt = DateTime.UtcNow;
                _dbSet.Update(verification);
                return true;
            }

            return false;
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