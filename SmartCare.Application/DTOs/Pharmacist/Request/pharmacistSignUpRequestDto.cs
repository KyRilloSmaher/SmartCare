using Microsoft.AspNetCore.Http;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Pharmacist.Request
{
    public class pharmacistSignUpRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public string userName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string LicenseNumber { get; set; }
        public Guid StoreId { get; set; }
    }
}
