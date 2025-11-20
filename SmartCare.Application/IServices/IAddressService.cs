using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Application.Handlers.ResponseHandler;


namespace SmartCare.Application.IServices
{
    public interface IAddressService
    {
        Task<Response<AddressResponseDto>> SetAddressAsPrimaryAddressAsync(string clientId, Guid addressId);
        Task<Response<AddressResponseDto>> AddNewClientAddressAsync(string clientId, CreateAddressRequestDto dto);
        Task<Response<IEnumerable<AddressResponseDto>>> GetClientAddressesAsync(string clientId);
        Task<Response<bool>> DeleteClientAddressAsync(string clientId, Guid addressId);
        Task<Response<AddressResponseDto>> UpdateClientAddressAsync(string clientId, UpdateAddressRequestDto dto);
    }
}
