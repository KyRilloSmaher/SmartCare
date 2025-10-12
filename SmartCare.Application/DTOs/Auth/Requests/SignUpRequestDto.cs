using Microsoft.AspNetCore.Http;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Auth.Requests
{
    public class SignUpRequest
    {
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateOnly BirthDate { get; set; }
        public Gender Gender { get; set; }
        public IFormFile ProfileImage { get; set; }
        public AccountType AccountType { get; set; }
        public CreateAddressRequestDto Address { get; set; }
    }
}
