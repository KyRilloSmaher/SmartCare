using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Favorites.Requests
{
    public class CreateFavouriteRequestDto
    {
        public Guid ProductId { get; set; }
        public string ClientId { get; set; }

    }
}
