using MediatR;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.ExternalServiceInterfaces.AI.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.AI.DrugInformationExtractor
{
  public record DrugInformationExtractorQuery(IFormFile Image) : IRequest<Response<DrugExtractionResponse>>;
}
