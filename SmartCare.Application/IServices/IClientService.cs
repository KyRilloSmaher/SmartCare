using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IClientService
    {
        Task<Response<ClientResponseDto?>> GetClientByIdAsync(string id);
        Task<Response<ClientResponseDto?>> GetClientByEmailAsync(string email);
        Task<Response<IEnumerable<ClientResponseDto>>> GetAllClientsAsync();
        Task<Response<ClientResponseDto?>> UpdateClientAsync(string Id ,UpdateClientRequest ClientDto);
        Task<Response<bool>> DeleteClientAsync(string id);
        Task<Response<string>> ChangeClientProfileImageAsync(string userId ,ChangeClientProfileImageRequestDto dto);

    }
}
