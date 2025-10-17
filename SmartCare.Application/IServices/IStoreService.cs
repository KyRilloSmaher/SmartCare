using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IStoreService
    {
        Task<Response<StoreResponseDto>> GetStoreByIdAsync(Guid Id);
        Task<Response<StoreResponseDto>> GetNearestStoreAsync(AddressValuesDto dto);
        Task<Response<IEnumerable<StoreResponseDto>>> SearchStoresByNameAsync(string name);
        Task<Response<IEnumerable<StoreResponseDto>>> GetAllStoresAsync();
        Task<Response<IEnumerable<StoreResponseForAdminDto>>> GetAllStoresForAdminAsync();
        Task<Response<StoreResponseForAdminDto>> CreateStoreAsync(CreateStoreRequestDto StoreDto);
        Task<Response<StoreResponseForAdminDto>> UpdateStoreAsync(Guid Id, UpdateStoreRequestDto StoreDto);
        Task<Response<bool>> DeleteStoreAsync(Guid Id);
    }
}
