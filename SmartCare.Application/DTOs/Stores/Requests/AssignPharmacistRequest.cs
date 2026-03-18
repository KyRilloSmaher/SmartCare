using Microsoft.AspNetCore.Http;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Stores.Requests
{
    public class AssignPharmacistRequest
    {
        // 🔹 User Data (ApplicationUser)
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Gender Gender { get; set; }
        public IFormFile? ProfileImage { get; set; }

        // 🔹 Pharmacist Data
        public string LicenseNumber { get; set; } = default!;
    }
}
