using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Client.Responses
{
    public class ClientResponseDto
    {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public Gender Gender { get; set; }
            public string ProfileImageUrl { get; set; } = string.Empty;
            public DateOnly BirthDate { get; set; }
            public string? Code { get; set; }
            public AccountType AccountType { get; set; }

            // Nested Collections (Optional summaries)
            public ICollection<CreateAddressResponseDto>? Addresses { get; set; }
        
    }
}
