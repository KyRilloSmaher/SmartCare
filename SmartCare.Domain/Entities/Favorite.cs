using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Favorite
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ClientId { get; set; }

        public Client Client { get; set; }

        public Product Product { get; set; }
    }
}
