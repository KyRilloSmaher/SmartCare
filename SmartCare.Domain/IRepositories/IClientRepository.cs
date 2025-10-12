using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.IRepositories
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        // Transaction Management
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Basic User Operations
        Task<Client?> GetByIdAsync(string ClientId, bool trackChanges = false);
        Task<Client?> GetByEmailAsync(string email, bool trackChanges = false);
        Task<Client?> GetByClientnameAsync(string Clientname, bool trackChanges = false);
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<IEnumerable<Client>> GetAllAdminsAsync();
        Task<IdentityResult> CreateClientAsync(Client Client, string password);
        Task<IdentityResult> UpdateClientAsync(Client Client);
        Task<IdentityResult> DeleteClientAsync(Client Client);

        // Client Verification
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsClientnameUniqueAsync(string Clientname);

        // Password Management
        Task<bool> CheckPasswordAsync(Client Client, string password);
        Task<bool> ChangePasswordAsync(Client Client, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(Client Client);
        Task<bool> ResetPasswordAsync(Client Client, string token, string newPassword);
        Task RemovePasswordAsync(Client Client);
        Task<bool> HasPasswordAsync(Client Client);
        Task<IdentityResult> AddPasswordAsync(Client Client, string password);
        // Role Management
        Task<IList<string>> GetClientRolesAsync(Client Client);
        Task<bool> AddToRoleAsync(Client Client, string role);
        Task<bool> RemoveFromRoleAsync(Client Client, string role);
        Task<bool> IsInRoleAsync(Client Client, string role);

        // Email Confirmation
        Task<string> GenerateEmailConfirmationTokenAsync(Client Client);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> IsEmailConfirmedAsync(Client Client);

        // Lockout/Account Status
        Task<bool> SetLockoutEnabledAsync(Client Client, bool enabled);
        Task<bool> IsLockedOutAsync(Client Client);
        Task<DateTimeOffset?> GetLockoutEndDateAsync(Client Client);
        Task<bool> SetLockoutEndDateAsync(Client Client, DateTimeOffset? lockoutEnd);
        Task<int> GetAccessFailedCountAsync(Client Client);
        Task<bool> ResetAccessFailedCountAsync(Client Client);

    }
}
