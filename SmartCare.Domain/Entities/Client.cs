
using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Enums;


namespace SmartCare.Domain.Entities
{
    public class Client: IdentityUser
    {
        public Gender Gender { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateOnly BirthDate { get; set; }
        public string? Code { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public AccountType AccountType { get; set; }
        public int RatesCount { get; set; } = 0;
        public int OrdersCount { get; set; } = 0;
        public int FavoritesCount { get; set; } = 0;
        public ICollection<Address> Addresses { get; set; }
        public  ICollection<Favorite> Favorites { get; set; }
        public  ICollection<Order> Orders { get; set; }
        public ICollection<Rate> Rates { get; set; }
        public  Cart Cart { get; set; }
    }
}
