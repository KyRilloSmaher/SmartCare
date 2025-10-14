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
        private readonly UserManager<Client> _ClientManager;
        private readonly ApplicationDBContext _context;
        #endregion

        #region Constructor(s)
        public ClientRepository(UserManager<Client> ClientManager, ApplicationDBContext context) : base(context)
        {
            _ClientManager = ClientManager ?? throw new ArgumentNullException(nameof(_ClientManager));
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
                ? await _ClientManager.Users.Include(u=>u.Addresses).SingleOrDefaultAsync(u => u.Id == ClientId)
                : await _ClientManager.Users.Include(u => u.Addresses).AsNoTracking().SingleOrDefaultAsync(u => u.Id == ClientId);
        }
        public async override Task<IEnumerable<Client>> GetAllAsync(bool AsTracking = false){ 
         return AsTracking
                ? await _ClientManager.Users.Include(u => u.Addresses).ToListAsync()
                : await _ClientManager.Users.Include(u => u.Addresses).AsNoTracking().ToListAsync();
        }

        public async Task<Client?> GetByEmailAsync(string email, bool trackChanges = false)
        {

            return trackChanges
                ? await _ClientManager.FindByEmailAsync(email)
                : await _ClientManager.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email);
        }
        public async Task<Client?> GetByClientnameAsync(string Clientname, bool trackChanges = false)
        {
            return trackChanges
                ? await _ClientManager.Users.SingleOrDefaultAsync(u => u.UserName == Clientname)
                : await _ClientManager.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == Clientname);
        }
        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _ClientManager.GetUsersInRoleAsync("User");
        }
        public async Task<IEnumerable<Client>> GetAllAdminsAsync()
        {
            return await _ClientManager.GetUsersInRoleAsync("Admin");
        }

        // Basic Client Operations

        public async Task<IdentityResult> CreateClientAsync(Client Client, string password)
        {
            var result = await _ClientManager.CreateAsync(Client, password);
            return result;
        }

        public async Task<IdentityResult> UpdateClientAsync(Client Client)
        {
            var result = await _ClientManager.UpdateAsync(Client);
            return result;
        }

        public async Task<IdentityResult> DeleteClientAsync(Client Client)
        {
            var result = await _ClientManager.DeleteAsync(Client);
            return result;
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            var Client = await _ClientManager.FindByEmailAsync(email);
            return Client == null;
        }

        public async Task<bool> IsClientnameUniqueAsync(string Clientname)
        {
            var Client = await _ClientManager.FindByNameAsync(Clientname);
            return Client == null;
        }

        public async Task<bool> CheckPasswordAsync(Client Client, string password)
        {
            return await _ClientManager.CheckPasswordAsync(Client, password);
        }

        public async Task<bool> ChangePasswordAsync(Client Client, string currentPassword, string newPassword)
        {
            var result = await _ClientManager.ChangePasswordAsync(Client, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Client Client)
        {
            return await _ClientManager.GeneratePasswordResetTokenAsync(Client);
        }

        public async Task<bool> ResetPasswordAsync(Client Client, string token, string newPassword)
        {
            var result = await _ClientManager.ResetPasswordAsync(Client, token, newPassword);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetClientRolesAsync(Client Client)
        {
            return await _ClientManager.GetRolesAsync(Client);
        }

        public async Task<bool> AddToRoleAsync(Client Client, string role)
        {
            var result = await _ClientManager.AddToRoleAsync(Client, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveFromRoleAsync(Client Client, string role)
        {
            var result = await _ClientManager.RemoveFromRoleAsync(Client, role);
            return result.Succeeded;
        }

        public async Task<bool> IsInRoleAsync(Client Client, string role)
        {
            return await _ClientManager.IsInRoleAsync(Client, role);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(Client Client)
        {
            return await _ClientManager.GenerateEmailConfirmationTokenAsync(Client);
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var Client = await GetByEmailAsync(email, true);

            if (Client != null)
            {
                var decodedToken = WebUtility.UrlDecode(token);
                var result = await _ClientManager.ConfirmEmailAsync(Client, token);

                if (result.Succeeded)
                {
                    Client.EmailConfirmed = true;
                    await _ClientManager.UpdateAsync(Client);
                }

                return result.Succeeded;
            }

            return false;
        }


        public async Task<bool> IsEmailConfirmedAsync(Client Client)
        {
            return await _ClientManager.IsEmailConfirmedAsync(Client);
        }

        public async Task<bool> SetLockoutEnabledAsync(Client Client, bool enabled)
        {
            var result = await _ClientManager.SetLockoutEnabledAsync(Client, enabled);
            return result.Succeeded;
        }

        public async Task<bool> IsLockedOutAsync(Client Client)
        {
            return await _ClientManager.IsLockedOutAsync(Client);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(Client Client)
        {
            return await _ClientManager.GetLockoutEndDateAsync(Client);
        }

        public async Task<bool> SetLockoutEndDateAsync(Client Client, DateTimeOffset? lockoutEnd)
        {
            var result = await _ClientManager.SetLockoutEndDateAsync(Client, lockoutEnd);
            return result.Succeeded;
        }

        public async Task<int> GetAccessFailedCountAsync(Client Client)
        {
            return await _ClientManager.GetAccessFailedCountAsync(Client);
        }

        public async Task<bool> ResetAccessFailedCountAsync(Client Client)
        {
            var result = await _ClientManager.ResetAccessFailedCountAsync(Client);
            return result.Succeeded;
        }

        // Additional custom methods not directly available in ClientManager
        public async Task<IEnumerable<Client>> GetClientsByRoleAsync(string role)
        {
            return await _ClientManager.GetUsersInRoleAsync(role);
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm)
        {
            return await _ClientManager.Users
                .Where(u =>
                    u.Email.Contains(searchTerm) ||
                    u.UserName.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task RemovePasswordAsync(Client Client)
        {
            await _ClientManager.RemovePasswordAsync(Client);
        }

        public async Task<bool> HasPasswordAsync(Client Client)
        {
            return await _ClientManager.HasPasswordAsync(Client);
        }

        public Task<IdentityResult> AddPasswordAsync(Client Client, string password)
        {
            var result = _ClientManager.AddPasswordAsync(Client, password);
            return result;
        }
        #endregion
    }
}
