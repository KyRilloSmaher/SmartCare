using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalPrice { get; set; }
        public Client  Client { get; set; }
        public  ICollection<CartItem> Items { get; set; }
    }
}
