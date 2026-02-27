using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class EmailVerification
    {

        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; private set; } = false;
        public DateTime UsedAt { get; private set; }

        public void markUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
        }

    }
}
