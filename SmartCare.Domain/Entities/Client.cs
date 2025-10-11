using Microsoft.AspNet.Identity.EntityFramework;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Client: IdentityUser
    {
        public Gender Gender { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateOnly birthDate { get; set; }
        public string? Code { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public AccountType AccountType { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public  ICollection<Favorite> Favorites { get; set; }
        public  ICollection<Order> Orders { get; set; }
        public ICollection<Rate> Rates { get; set; }
        public  Cart Cart { get; set; }
    }
}
