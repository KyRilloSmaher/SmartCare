using MediatR;
using SmartCare.Application.DTOs.Analytics.Categories;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.Analytics.Categories
{

        public class GetCategoryPerformanceQuery : IRequest<Response<List<CategoryPerformanceDto>>>
        {
            public Guid? BranchId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }
    
}
