using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Application.Features.AI.Chat;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.AI.DrugInformationExtractor
{
    internal class DrugInformationExtractorQueryHandler : IRequestHandler<DrugInformationExtractorQuery, Response<DrugExtractionResponse>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<DrugInformationExtractorQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiServices _aiServices;
        private readonly IMapper _mapper;
        public DrugInformationExtractorQueryHandler(IResponseHandler responseHandler, ILogger<DrugInformationExtractorQueryHandler> logger, IUnitOfWork unitOfWork, IAiServices aiServices, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aiServices = aiServices;
            _mapper = mapper;
        }


        public async Task<Response<DrugExtractionResponse>> Handle(DrugInformationExtractorQuery request, CancellationToken cancellationToken)
        {
            var response = await _aiServices.DrugInformationExtractorAsync(request.Image,cancellationToken);
            if (response == null)
            {
                return _responseHandler.Failed<DrugExtractionResponse>("Failed to get a response from the AI service.");
            }
            return _responseHandler.Success(response);
        }
    }
}
