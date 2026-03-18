using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Stores.Responses
{
    public class PharmacistResponseDto
    {
        public string Id { get; set; }
        public string FullName { get; set; } = default!;
        public string PharmacistEmail { get; set; } = default!;
        public string PharmacistUserName { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Licence { get; set; } = default!;
        public Guid BranchId { get; set; }
    }
}
