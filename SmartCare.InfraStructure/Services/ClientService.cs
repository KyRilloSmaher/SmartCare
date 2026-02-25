using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.InfraStructure.Services
{
    public class ClientService : IClientService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRateRepository _rateRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Client;
        #endregion

        #region Constructor
        public ClientService(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            UserManager<ApplictionUser> userManager,
            IRedisCacheService redisCacheService,
            IRateRepository rateRepository,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _userManager = userManager;
            _redisCacheService = redisCacheService;
            _rateRepository = rateRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        #endregion

        #region Methods

        public async Task<Response<ClientResponseDto?>> GetClientByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_id_{id}";
            try
            {
                var cached = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cached != null) return _responseHandler.Success(cached);
            }
            catch { }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var dto = _mapper.Map<ClientResponseDto?>(user);
            await _redisCacheService.SetDataAsync(cacheKey, dto, tag, Time.Default);

            return _responseHandler.Success(dto);
        }

        public async Task<Response<ClientResponseDto?>> GetClientByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_email_{email.ToLower()}";
            try
            {
                var cached = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cached != null) return _responseHandler.Success(cached);
            }
            catch { }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var dto = _mapper.Map<ClientResponseDto?>(user);
            await _redisCacheService.SetDataAsync(cacheKey, dto, tag, Time.Default);

            return _responseHandler.Success(dto);
        }

        public async Task<Response<IEnumerable<ClientResponseDto>>> GetAllClientsAsync()
        {
            string cacheKey = "clients_all";
            try
            {
                var cached = await _redisCacheService.GetDataAsync<IEnumerable<ClientResponseDto>>(cacheKey, tag);
                if (cached != null) return _responseHandler.Success(cached);
            }
            catch { }

            var users = _userManager.Users.ToList();
            var dtos = _mapper.Map<IEnumerable<ClientResponseDto>>(users);

            await _redisCacheService.SetDataAsync(cacheKey, dtos, tag, Time.Default);
            return _responseHandler.Success(dtos);
        }

        public async Task<Response<ClientResponseDto?>> UpdateClientAsync(string id, UpdateClientRequest dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var oldEmail = user.Email;
            _mapper.Map(dto, user);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return _responseHandler.Failed<ClientResponseDto?>(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _redisCacheService.RemoveKeyAsync($"client_id_{id}", tag);
            if (!string.IsNullOrEmpty(oldEmail))
                await _redisCacheService.RemoveKeyAsync($"client_email_{oldEmail.ToLower()}", tag);
            await _redisCacheService.RemoveKeyAsync("clients_all", tag);
            await _redisCacheService.DeleteKeysByTag(tag);

            var userDto = _mapper.Map<ClientResponseDto?>(user);
            return _responseHandler.Success(userDto);
        }

        public async Task<Response<bool>> DeleteClientAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<bool>(SystemMessages.USER_NOT_FOUND);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return _responseHandler.NotFound<bool>(SystemMessages.USER_NOT_FOUND);

            var imageUrl = user.ProfileImageUrl;

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);

            if (!string.IsNullOrEmpty(imageUrl))
                await _imageUploaderService.DeleteImageByUrlAsync(imageUrl);

            await _redisCacheService.RemoveKeyAsync($"client_id_{id}", tag);
            await _redisCacheService.RemoveKeyAsync($"client_email_{user.Email?.ToLower()}", tag);
            await _redisCacheService.RemoveKeyAsync("clients_all", tag);

            _backgroundJobService.Enqueue(() => _rateRepository.MarkAllClientRatesAsDeleted(id));

            return _responseHandler.Success(true, SystemMessages.SUCCESS);
        }

        public async Task<Response<string>> ChangeClientProfileImageAsync(string userId, ChangeClientProfileImageRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<string>(SystemMessages.USER_NOT_FOUND);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return _responseHandler.NotFound<string>(SystemMessages.USER_NOT_FOUND);

            var oldUrl = user.ProfileImageUrl;
            if (!string.IsNullOrEmpty(oldUrl))
                await _imageUploaderService.DeleteImageByUrlAsync(oldUrl);

            var upload = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);
            if (upload.Error != null)
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);

            user.ProfileImageUrl = upload.Url.ToString();
            await _userManager.UpdateAsync(user);

            await _redisCacheService.RemoveKeyAsync($"client_id_{userId}", tag);

            return _responseHandler.Success(user.ProfileImageUrl);
        }

        #endregion
    }
}