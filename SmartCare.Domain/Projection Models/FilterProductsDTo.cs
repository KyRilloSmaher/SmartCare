using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Projection_Models
{
    public class FilterProductsDTo
    {
       public string? companyName {  get; set; }
       public string? categoryName { get; set; }
       public float? FromRate {  get; set; }
       public float? ToRate {  get; set; }
       public decimal? FromPrice {  get; set; }
       public decimal? ToPrice {  get; set; }
    }
}
