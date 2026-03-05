using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Configurations
{
    public class FromStoreOrderConfiguration : IEntityTypeConfiguration<PickUpOrder>
    {
        public void Configure(EntityTypeBuilder<PickUpOrder> builder)
        {
            builder.ToTable("FromStoreOrders");

            builder.HasOne(o => o.Store)
                   .WithMany()
                   .HasForeignKey(o => o.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.PickupCodeHash)
                   .IsRequired(false);

            builder.HasIndex(o => o.PickupCodeHash);
        }
    }
}
