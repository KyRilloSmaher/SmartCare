using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Application.Features.Product.Queries.SearchInProducts;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.AI.Chat
{
    public class AskAIQueryHandler : IRequestHandler<AskAIQuery, Response<AiAnswerResult>>
    {
        private readonly IResponseHandler _responseHandler;
        private readonly IMediator _mediator;
        private readonly ILogger<AskAIQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiServices _aiServices;
        private readonly IMapper _mapper;
        public AskAIQueryHandler(IResponseHandler responseHandler, IMediator mediator, ILogger<AskAIQueryHandler> logger, IUnitOfWork unitOfWork, IAiServices aiServices, IMapper mapper)
        {
            _responseHandler = responseHandler;
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aiServices = aiServices;
            _mapper = mapper;
        }


        public async Task<Response<AiAnswerResult>> Handle(AskAIQuery request, CancellationToken cancellationToken)
        {
            var response = await _aiServices.AskAIAsync(request.TextQuestion, request.ingredient,request.AudioFile);
            if (response == null)
            {
                return _responseHandler.Failed<AiAnswerResult>("Failed to get a response from the AI service.");
            }
            return _responseHandler.Success(response);
        }
    }
}
