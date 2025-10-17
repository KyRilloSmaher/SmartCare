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
            public string Id { get; set; } 
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Gender { get; set; }
            public string ProfileImageUrl { get; set; }
            public DateOnly BirthDate { get; set; }
            public string AccountType { get; set; }
            public int RatesCount { get; set; }
            public int FavoritesCount { get; set; }
            public int OrdersCount { get; set; }

        // Nested Collections (Optional summaries)
        public ICollection<AddressResponseDto>? Addresses { get; set; }
        
    }
}
