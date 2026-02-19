using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Address.Extensions
{
    public static class AddressExtensions
    {
        public static async Task<Client?> GetValidClientAsync(this IClientRepository clientRepository, string clientId)
        {
            return await clientRepository.GetByIdAsync(clientId);
        }

        public static async Task<Response<T>> ClientNotFoundAsync<T>(this IResponseHandler responseHandler)
        {
            return responseHandler.NotFound<T>(SystemMessages.USER_NOT_FOUND);
        }

        public static async Task HandlePrimaryAddressChangeAsync(
            this IAddressRepository addressRepository,
            string clientId,
            SmartCare.Domain.Entities.Address newPrimary)
        {
            var currentPrimary = await addressRepository.GetPrimaryAddressAsync(clientId);

            if (currentPrimary != null && currentPrimary.Id != newPrimary.Id)
            {
                currentPrimary.IsPrimary = false;
                await addressRepository.UpdateAsync(currentPrimary);
            }

            newPrimary.IsPrimary = true;
        }
    }
}
