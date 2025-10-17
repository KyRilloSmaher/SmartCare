using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Caregory.Response
{
    public class CategoryResponseForAdminDto : CategoryResponseDto
    {
       public bool IsDeleted { get; set; }
    }
}
