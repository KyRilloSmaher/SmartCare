using SmartCare.Application.DTOs.Product.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Contradictions.Response
{
    /// <summary>
    /// Extended DTO for products with contradiction details
    /// </summary>
    public class ProductWithContradictionsDto : ProductResponseDtoForClient
    {
        public List<ContradictionDetail> ContradictionDetails { get; set; } = new();
    }
}
