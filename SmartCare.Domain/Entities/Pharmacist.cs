using Microsoft.AspNetCore.Identity;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Pharmacist
    {
        public string Id { get; set; }
        public string LicenseNumber {  get; set; }
        public bool IsActive { get; set; }
        public Guid StoreId { get; set; }
        public Store Store { get; set; }
        public ApplictionUser User { get; set; }
    }
}
