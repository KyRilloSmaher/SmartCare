using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using SmartCare.InfraStructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SmartCare.InfraStructure.Repositories
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        #region Feild(s)
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor(s)
        public ClientRepository(ApplicationDBContext context) : base(context)
        {
          
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        #endregion

        #region Method(s)
        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task<Client?> GetByIdAsync(string ClientId, bool trackChanges = false)
        {
            return trackChanges
                ? await _context.Clients.Include(u=>u.Addresses).SingleOrDefaultAsync(u => u.Id == ClientId)
                : await _context.Clients.Include(u => u.Addresses).AsNoTracking().SingleOrDefaultAsync(u => u.Id == ClientId);
        }
        public async override Task<IEnumerable<Client>> GetAllAsync(bool AsTracking = false){ 
         return AsTracking
                ? await _context.Clients.Include(u => u.Addresses).ToListAsync()
                : await _context.Clients.Include(u => u.Addresses).AsNoTracking().ToListAsync();
        }

       
        public async Task<bool> IsClientPhoneNumberUniqueAsync(string phone)
        {
        
            bool exists = await _context.Clients.Include(c=>c.User).AnyAsync(c => c.User.PhoneNumber == phone);
            return !exists; 
        }


        public async Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm)
        {
            return await _context.Clients.Include(c => c.User)
                .Where(u =>
                    u.User.Email.Contains(searchTerm) ||
                    u.User.UserName.Contains(searchTerm))
                .ToListAsync();
        }

        #endregion
    }
}
