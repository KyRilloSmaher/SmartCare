using Microsoft.AspNetCore.Http;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Pharmacist.Request
{
    public class pharmacistSignUpRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string userName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateOnly BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public IFormFile? ProfileImage { get; set; }
        [RegularExpression(@"PH-\d{2}-[A-Z]{3}-\d{4}$", ErrorMessage ="Invalid License Number format. Ex: PH-26-CAI-0842")]
        public string LicenseNumber { get; set; }
        public Guid StoreId { get; set; }
    }
}
