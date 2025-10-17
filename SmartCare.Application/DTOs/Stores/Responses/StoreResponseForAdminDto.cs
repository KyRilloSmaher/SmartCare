using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Stores.Responses
{
    public class StoreResponseForAdminDto :StoreResponseDto
    {
        public bool IsDeleted { get; set; }
    }
}
