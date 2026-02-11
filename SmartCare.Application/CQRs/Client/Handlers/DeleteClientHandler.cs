using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class DeleteClientHandler : IRequestHandler<DeleteClientAsyncCommand, Response<bool>>
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


        public DeleteClientHandler(
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
        public async Task<Response<bool>> Handle(DeleteClientAsyncCommand request, CancellationToken cancellationToken)
        {
            var id = request.id;
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
                        await _redisCacheService.RemoveKeyAsync("clients_all", tag);
                        await _redisCacheService.RemoveKeyAsync($"client_email_{client.Email?.ToLower()}", tag);

                        _backgroundJobService.Enqueue(() => _rateRepository.MarkAllClientRatesAsDeleted(id));
                        return _responseHandler.Success<bool>(true, SystemMessages.SUCCESS);
                    }
                }
                throw new Exception();

            }
            catch (Exception ex)
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}
