using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Rate
    {
        public Guid Id { get; set; }
        public string? ClientId { get; set; }
        public Guid ProductId { get; set; }
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Client Client { get; set; }
        public Product Product { get; set; }

    }
}
