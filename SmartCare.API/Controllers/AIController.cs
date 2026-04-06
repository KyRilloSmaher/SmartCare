using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.ExternalServiceInterfaces.AI.Request;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Application.Features.AI.Chat;
using SmartCare.Application.Features.Product.Queries.VoiceSearch;
using SmartCare.Application.Handlers.ResponseHandler;

namespace SmartCare.API.Controllers
{
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AIController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Chat With the AI Medical Assistant to get answers for your medical questions.
        /// </summary>
        [HttpPost(ApplicationRouting.AI.chat)]
        [ProducesResponseType(typeof(Response<AiAnswerResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VoiceSearch([FromForm] AskAIRequest request)
        {
            var result = await _mediator.Send(new AskAIQuery(request.AudioFile,request.TextQuestion, request.ingredient));
            return ControllersHelperMethods.FinalResponse(result);
        }
    }
}
