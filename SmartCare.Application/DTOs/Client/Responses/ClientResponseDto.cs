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
            public string Gender { get; set; }
            public string ProfileImageUrl { get; set; } = string.Empty;
            public DateOnly BirthDate { get; set; }
            public string AccountType { get; set; }
            public int RatesCount { get; set; }
            public int FavoritesCount { get; set; }
            public int OrdersCount { get; set; }

        // Nested Collections (Optional summaries)
        public ICollection<AddressResponseDto>? Addresses { get; set; }
        
    }
}
