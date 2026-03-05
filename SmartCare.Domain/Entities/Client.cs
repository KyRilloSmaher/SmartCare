using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Client 
    {
        public string Id { get; set; }
        public AccountType AccountType { get; set; }
        public DateOnly BirthDate { get; set; }
        public int RatesCount { get; set; } = 0;
        public int OrdersCount { get; set; } = 0;
        public int FavoritesCount { get; set; } = 0;
        public ICollection<Address> Addresses { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Rate> Rates { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ApplictionUser User { get; set; }
    }
}
