using AutoMapper;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;


namespace SmartCare.InfraStructure.Services
{
    public class ClientService : IClientService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IClientRepository _clientRepository;
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
            IClientRepository clientRepository,
            IRedisCacheService redisCacheService,
            IRateRepository rateRepository,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _rateRepository = rateRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        #endregion

        #region Methods
        public async  Task<Response<ClientResponseDto?>> GetClientByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_id_{id}";

            try
            {
                var cachedClient = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cachedClient != null) return _responseHandler.Success(cachedClient);
            }
            catch (Exception) { /* Redis logic shouldn't break the app */ }

            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var clientDto = _mapper.Map<ClientResponseDto?>(client);

            await _redisCacheService.SetDataAsync(cacheKey, clientDto, tag, Time.Default);
            return _responseHandler.Success(clientDto);
        }

        public async Task<Response<ClientResponseDto?>> GetClientByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_email_{email.ToLower()}"; 

            try
            {
                var cachedClient = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cachedClient != null) return _responseHandler.Success(cachedClient);
            }
            catch (Exception) { }

            var client = await _clientRepository.GetByEmailAsync(email);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var clientDto = _mapper.Map<ClientResponseDto?>(client);

            await _redisCacheService.SetDataAsync(cacheKey, clientDto, tag, Time.Default);
            return _responseHandler.Success(clientDto);
        }

        public async Task<Response<IEnumerable<ClientResponseDto>>> GetAllClientsAsync()
        {
            string cacheKey = "clients_all";

            try
            {
                var cachedClients = await _redisCacheService.GetDataAsync<IEnumerable<ClientResponseDto>>(cacheKey, tag);
                if (cachedClients != null) return _responseHandler.Success(cachedClients);
            }
            catch (Exception) { }

            var clients = await _clientRepository.GetAllAsync();
            var clientDtos = _mapper.Map<IEnumerable<ClientResponseDto>>(clients);

            await _redisCacheService.SetDataAsync(cacheKey, clientDtos, tag, Time.Default); 
            return _responseHandler.Success(clientDtos);
        }

        public async Task<Response<ClientResponseDto?>> UpdateClientAsync(string Id, UpdateClientRequest ClientDto)
        {
            if (string.IsNullOrWhiteSpace(Id))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);
            var client = await _clientRepository.GetByIdAsync(Id, true);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);
            var oldEmail = client.Email;
            _mapper.Map(ClientDto, client);
            var updatedClient = await _clientRepository.UpdateAsync(client);

            var key = $"client_id_{Id}";
            await _redisCacheService.RemoveKeyAsync($"client_id_{Id}" ,tag);
            if (!string.IsNullOrEmpty(oldEmail))
                await _redisCacheService.RemoveKeyAsync($"client_email_{oldEmail.ToLower()}" , tag);

            await _redisCacheService.RemoveKeyAsync("clients_all", tag);
            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedClientDto = _mapper.Map<ClientResponseDto?>(updatedClient);
            return _responseHandler.Success(updatedClientDto);
        }

        public async Task<Response<bool>> DeleteClientAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return _responseHandler.BadRequest<bool>(SystemMessages.USER_NOT_FOUND);
                await _clientRepository.BeginTransactionAsync();
                var client = await _clientRepository.GetByIdAsync(id, true);
                if (client == null)
                    return _responseHandler.NotFound<bool>(SystemMessages.USER_NOT_FOUND);
                // Get profile image URl 
                var imageUrl = client.ProfileImageUrl;
                var deleteResult = await _clientRepository.DeleteClientAsync(client);

                if (deleteResult.Succeeded)
                {
                    var DeleteImageResult = await _imageUploaderService.DeleteImageByUrlAsync(imageUrl);
                    if (DeleteImageResult)
                    {
                        await _clientRepository.CommitTransactionAsync();

                        await _redisCacheService.RemoveKeyAsync($"client_id_{id}", tag);
                        await _redisCacheService.RemoveKeyAsync("clients_all",tag);
                        await _redisCacheService.RemoveKeyAsync($"client_email_{client.Email?.ToLower()}" , tag);

                        _backgroundJobService.Enqueue(() => _rateRepository.MarkAllClientRatesAsDeleted(id));
                        return _responseHandler.Success<bool>(true, SystemMessages.SUCCESS);
                    }
                }
                throw new Exception();

            }
            catch(Exception ex)
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }

        public async Task<Response<string>> ChangeClientProfileImageAsync( string UserId ,ChangeClientProfileImageRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return _responseHandler.BadRequest<string>(SystemMessages.USER_NOT_FOUND);
            var client = await _clientRepository.GetByIdAsync(UserId , true);
            if (client == null)
                return _responseHandler.NotFound<string>(SystemMessages.USER_NOT_FOUND);
            // Delete old image 
             var oldImageUrl = client.ProfileImageUrl;
             var DeleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!DeleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);
            var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage , ImageFolder.UserProfiles);
            if (uploadResult.Error != null)
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }
            client.ProfileImageUrl = uploadResult.Url.ToString();
            var updateResult = await _clientRepository.UpdateAsync(client);
            string cacheKey = $"client_id_{UserId}";
            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);
            return _responseHandler.Success(updateResult.ProfileImageUrl);
        }
        #endregion
    }
}
