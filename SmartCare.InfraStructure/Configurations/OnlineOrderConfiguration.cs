using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class OnlineOrderConfiguration : IEntityTypeConfiguration<OnlineOrder>
    {
        public void Configure(EntityTypeBuilder<OnlineOrder> builder)
        {

            builder.ToTable("OnlineOrders");

            builder.HasOne(o => o.Address)
                   .WithMany()
                   .HasForeignKey(o => o.ShippingAddressId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
