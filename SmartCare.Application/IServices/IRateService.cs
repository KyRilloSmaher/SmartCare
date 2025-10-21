using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IRateService
    {
        Task<Response<RateResponseDto>> GetRateByIdAsync(Guid Id);
        Task<Response<IEnumerable<RateResponseDto>>> GetAllRatesForUserAsync(string userId);
        Task<Response<IEnumerable<RateResponseDto>>> GetAllRatesForProductAsync(Guid Id);
        Task<Response<RateResponseDto>> CreateRateAsync(string Id,CreateRateRequestDto Dto);
        Task<Response<RateResponseDto>> UpdateRateAsync(string Id, UpdateRateRequestDto Dto);
        Task<Response<bool>> DeleteRateAsync(string userId ,Guid Id);
    }
}

