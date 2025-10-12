using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Auth.Requests
{
    public class ConfirmResetPasswordCodeRequestDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
