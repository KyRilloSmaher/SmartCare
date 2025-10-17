using SmartCare.Application.DTOs.Companies.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Companies.Responses
{
    public class CompanyResponseForAdminDto : CompanyResponseDto
    {
       public bool IsDeleted { get; set; }
    }
}
